using System.Diagnostics;
using System.Net;
using Api.Comercial.Data;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using A = DocumentFormat.OpenXml.Drawing;

namespace Api.Comercial.Services;

public interface IProposalDocumentService
{
    Task<OperationResult<ProposalPdfResult>> ExportPdfAsync(int proposalId, CancellationToken cancellationToken);
}

public sealed record ProposalPdfResult(string FileName, byte[] Content);

public sealed class ProposalDocumentService : IProposalDocumentService
{
    private const long EmuPerInch = 914400;
    private const double EmuPerCm = 360000d;
    private const float DefaultDpi = 96f;

    private readonly ApeironDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public ProposalDocumentService(ApeironDbContext context, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _context = context;
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<OperationResult<ProposalPdfResult>> ExportPdfAsync(int proposalId, CancellationToken cancellationToken)
    {
        var proposal = await _context.Proposals
            .AsNoTracking()
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == proposalId && p.Active, cancellationToken);

        if (proposal is null || proposal.Client is null || !proposal.Client.Active)
        {
            return OperationResult<ProposalPdfResult>.Fail("not_found", "Proposal not found.");
        }

        if (string.IsNullOrWhiteSpace(proposal.Client.LogoUrl))
        {
            return OperationResult<ProposalPdfResult>.Fail("domain_error", "Client logo is required to export proposal PDF.");
        }

        var templatePath = ResolveTemplatePath();
        if (!File.Exists(templatePath))
        {
            return OperationResult<ProposalPdfResult>.Fail("domain_error", $"Official template not found at '{templatePath}'.");
        }

        var logoResult = await TryReadLogoBytesAsync(proposal.Client.LogoUrl!, cancellationToken);
        if (!logoResult.Success)
        {
            return OperationResult<ProposalPdfResult>.Fail(logoResult.ErrorCode!, logoResult.ErrorMessage!);
        }

        var workDir = Path.Combine(Path.GetTempPath(), "api-comercial-pdf", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);

        var workDocx = Path.Combine(workDir, $"proposal-{proposal.Id}.docx");
        var expectedPdf = Path.Combine(workDir, $"proposal-{proposal.Id}.pdf");

        try
        {
            File.Copy(templatePath, workDocx, overwrite: true);

            var compose = ComposeDocument(workDocx, proposal, logoResult.Data!);
            if (!compose.Success)
            {
                return OperationResult<ProposalPdfResult>.Fail(compose.ErrorCode!, compose.ErrorMessage!);
            }

            var convert = await ConvertDocxToPdfAsync(workDocx, workDir, cancellationToken);
            if (!convert.Success)
            {
                return OperationResult<ProposalPdfResult>.Fail(convert.ErrorCode!, convert.ErrorMessage!);
            }

            if (!File.Exists(expectedPdf))
            {
                return OperationResult<ProposalPdfResult>.Fail("domain_error", "PDF output file was not generated.");
            }

            var pdfBytes = await File.ReadAllBytesAsync(expectedPdf, cancellationToken);
            return OperationResult<ProposalPdfResult>.Ok(new ProposalPdfResult($"proposta-{proposal.Id}.pdf", pdfBytes));
        }
        catch (Exception ex)
        {
            return OperationResult<ProposalPdfResult>.Fail("error", $"Unexpected export error: {ex.GetBaseException().Message}");
        }
        finally
        {
            try
            {
                if (Directory.Exists(workDir))
                {
                    Directory.Delete(workDir, recursive: true);
                }
            }
            catch
            {
            }
        }
    }

    private string ResolveTemplatePath()
        => Path.Combine(_environment.ContentRootPath, "docs", "templates", "Modelo_proposta_comercial.docx");

    private async Task<OperationResult<byte[]>> TryReadLogoBytesAsync(string logoUrl, CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(logoUrl, UriKind.Absolute, out var absoluteUri))
        {
            try
            {
                using var httpClient = new HttpClient();
                var bytes = await httpClient.GetByteArrayAsync(absoluteUri, cancellationToken);
                return bytes.Length == 0
                    ? OperationResult<byte[]>.Fail("domain_error", "Client logo file is empty.")
                    : OperationResult<byte[]>.Ok(bytes);
            }
            catch (Exception ex)
            {
                return OperationResult<byte[]>.Fail("domain_error", $"Unable to read client logo: {ex.GetBaseException().Message}");
            }
        }

        var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var relative = logoUrl.Trim().TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(webRoot, relative);

        if (!File.Exists(fullPath))
        {
            return OperationResult<byte[]>.Fail("domain_error", "Client logo file was not found in storage.");
        }

        var data = await File.ReadAllBytesAsync(fullPath, cancellationToken);
        return data.Length == 0
            ? OperationResult<byte[]>.Fail("domain_error", "Client logo file is empty.")
            : OperationResult<byte[]>.Ok(data);
    }

    private static OperationResult<bool> ComposeDocument(string docxPath, Proposal proposal, byte[] logoBytes)
    {
        using var document = WordprocessingDocument.Open(docxPath, true);
        if (document.MainDocumentPart?.Document is null)
        {
            return OperationResult<bool>.Fail("domain_error", "Template is invalid: missing main document part.");
        }

        var logoReplace = ReplaceClientLogoNearFirstClientName(document, logoBytes);
        if (!logoReplace.Success)
        {
            return logoReplace;
        }

        var clientReplace = ReplaceClientNamePlaceholders(document, proposal.Client!.Name);
        if (!clientReplace.Success)
        {
            return clientReplace;
        }

        var subjectReplace = ReplaceSubjectPlaceholder(document, proposal.ObjectiveHtml);
        if (!subjectReplace.Success)
        {
            return subjectReplace;
        }

        EnsureUpdateFieldsOnOpen(document);
        document.MainDocumentPart.Document.Save();
        return OperationResult<bool>.Ok(true);
    }

    private static void EnsureUpdateFieldsOnOpen(WordprocessingDocument document)
    {
        var settingsPart = document.MainDocumentPart?.DocumentSettingsPart
            ?? document.MainDocumentPart?.AddNewPart<DocumentSettingsPart>();
        if (settingsPart is null)
        {
            return;
        }

        settingsPart.Settings ??= new Settings();

        var update = settingsPart.Settings.Elements<UpdateFieldsOnOpen>().FirstOrDefault();
        if (update is null)
        {
            settingsPart.Settings.AppendChild(new UpdateFieldsOnOpen { Val = true });
        }
        else
        {
            update.Val = true;
        }

        settingsPart.Settings.Save();
    }

    private static OperationResult<bool> ReplaceClientNamePlaceholders(WordprocessingDocument document, string clientName)
    {
        var replaced = false;
        var roots = new List<OpenXmlElement> { document.MainDocumentPart!.Document };
        roots.AddRange(document.MainDocumentPart.HeaderParts.Where(h => h.Header is not null).Select(h => (OpenXmlElement)h.Header!));
        roots.AddRange(document.MainDocumentPart.FooterParts.Where(f => f.Footer is not null).Select(f => (OpenXmlElement)f.Footer!));

        foreach (var root in roots)
        {
            foreach (var text in root.Descendants<Text>())
            {
                if (string.IsNullOrWhiteSpace(text.Text))
                {
                    continue;
                }

                var updated = text.Text.Replace("[ClientName]", clientName, StringComparison.OrdinalIgnoreCase);
                if (!string.Equals(updated, text.Text, StringComparison.Ordinal))
                {
                    text.Text = updated;
                    replaced = true;
                }
            }
        }

        return replaced
            ? OperationResult<bool>.Ok(true)
            : OperationResult<bool>.Fail("domain_error", "Template is invalid: token [ClientName] was not found.");
    }

    private static OperationResult<bool> ReplaceSubjectPlaceholder(WordprocessingDocument document, string objectiveHtml)
    {
        var body = document.MainDocumentPart!.Document.Body;
        if (body is null)
        {
            return OperationResult<bool>.Fail("domain_error", "Template is invalid: missing body.");
        }

        var subjectToken = document.MainDocumentPart.Document.Descendants<Text>()
            .FirstOrDefault(t => (t.Text ?? string.Empty).Contains("[Subject]", StringComparison.OrdinalIgnoreCase));

        if (subjectToken is null)
        {
            return OperationResult<bool>.Fail("domain_error", "Template is invalid: token [Subject] was not found.");
        }

        var paragraphs = BuildObjectiveParagraphs(objectiveHtml);
        if (paragraphs.Count == 0)
        {
            paragraphs.Add(new Paragraph(new Run(new Text(string.Empty))));
        }

        var ownerParagraph = subjectToken.Ancestors<Paragraph>().FirstOrDefault();
        if (ownerParagraph is null)
        {
            return OperationResult<bool>.Fail("domain_error", "Template is invalid: [Subject] must be inside a paragraph.");
        }

        if (string.Equals((ownerParagraph.InnerText ?? string.Empty).Trim(), "[Subject]", StringComparison.OrdinalIgnoreCase))
        {
            OpenXmlElement current = ownerParagraph;
            foreach (var paragraph in paragraphs)
            {
                current = body.InsertAfter(paragraph, current);
            }

            ownerParagraph.Remove();
        }
        else
        {
            subjectToken.Text = subjectToken.Text.Replace("[Subject]", HtmlToPlainText(objectiveHtml), StringComparison.OrdinalIgnoreCase);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static List<Paragraph> BuildObjectiveParagraphs(string objectiveHtml)
    {
        var result = new List<Paragraph>();
        var html = string.IsNullOrWhiteSpace(objectiveHtml) ? "<p></p>" : objectiveHtml;

        var parser = new HtmlDocument();
        parser.LoadHtml($"<root>{html}</root>");
        var root = parser.DocumentNode.SelectSingleNode("//root");
        if (root is null)
        {
            return result;
        }

        foreach (var node in root.ChildNodes)
        {
            AppendBlockNode(node, result);
        }

        return result;
    }

    private static void AppendBlockNode(HtmlNode node, List<Paragraph> output)
    {
        if (node.NodeType == HtmlNodeType.Comment)
        {
            return;
        }

        if (node.Name.Equals("ul", StringComparison.OrdinalIgnoreCase) || node.Name.Equals("ol", StringComparison.OrdinalIgnoreCase))
        {
            var isOrdered = node.Name.Equals("ol", StringComparison.OrdinalIgnoreCase);
            var index = 1;
            foreach (var li in node.Elements("li"))
            {
                var runs = new List<Run> { new(new Text(isOrdered ? $"{index}. " : "- ")) };
                foreach (var child in li.ChildNodes)
                {
                    AppendInlineNode(child, null, runs);
                }

                output.Add(new Paragraph(runs));
                index++;
            }

            return;
        }

        if (node.Name.Equals("p", StringComparison.OrdinalIgnoreCase)
            || node.Name.Equals("div", StringComparison.OrdinalIgnoreCase)
            || node.NodeType == HtmlNodeType.Text)
        {
            var runs = new List<Run>();
            if (node.NodeType == HtmlNodeType.Text)
            {
                AppendInlineNode(node, null, runs);
            }
            else
            {
                foreach (var child in node.ChildNodes)
                {
                    AppendInlineNode(child, null, runs);
                }
            }

            if (runs.Count == 0)
            {
                runs.Add(new Run(new Text(string.Empty)));
            }

            output.Add(new Paragraph(runs));
            return;
        }

        foreach (var child in node.ChildNodes)
        {
            AppendBlockNode(child, output);
        }
    }

    private static void AppendInlineNode(HtmlNode node, RunProperties? currentProps, List<Run> runs)
    {
        if (node.NodeType == HtmlNodeType.Comment)
        {
            return;
        }

        if (node.Name.Equals("br", StringComparison.OrdinalIgnoreCase))
        {
            var props = CloneRunProperties(currentProps);
            runs.Add(props is null ? new Run(new Break()) : new Run(props, new Break()));
            return;
        }

        if (node.NodeType == HtmlNodeType.Text)
        {
            var text = WebUtility.HtmlDecode(node.InnerText ?? string.Empty);
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var props = CloneRunProperties(currentProps);
            var textElement = new Text(text) { Space = SpaceProcessingModeValues.Preserve };
            runs.Add(props is null ? new Run(textElement) : new Run(props, textElement));
            return;
        }

        var nextProps = CloneRunProperties(currentProps);
        if (node.Name.Equals("strong", StringComparison.OrdinalIgnoreCase) || node.Name.Equals("b", StringComparison.OrdinalIgnoreCase))
        {
            nextProps ??= new RunProperties();
            nextProps.Bold = new Bold();
        }

        if (node.Name.Equals("em", StringComparison.OrdinalIgnoreCase) || node.Name.Equals("i", StringComparison.OrdinalIgnoreCase))
        {
            nextProps ??= new RunProperties();
            nextProps.Italic = new Italic();
        }

        if (node.Name.Equals("u", StringComparison.OrdinalIgnoreCase))
        {
            nextProps ??= new RunProperties();
            nextProps.Underline = new Underline { Val = UnderlineValues.Single };
        }

        foreach (var child in node.ChildNodes)
        {
            AppendInlineNode(child, nextProps, runs);
        }
    }

    private static RunProperties? CloneRunProperties(RunProperties? source)
        => source is null ? null : (RunProperties)source.CloneNode(true);

    private static string HtmlToPlainText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var parser = new HtmlDocument();
        parser.LoadHtml(html);
        return WebUtility.HtmlDecode(parser.DocumentNode.InnerText ?? string.Empty).Trim();
    }

    private static OperationResult<bool> ReplaceClientLogoNearFirstClientName(WordprocessingDocument document, byte[] logoBytes)
    {
        var anchor = FindFirstClientNameAnchor(document);
        if (anchor is null)
        {
            return OperationResult<bool>.Fail("domain_error", "Template is invalid: token [ClientName] not found for logo anchoring.");
        }

        if (!TryReplaceClosestImage(anchor.Value.Part, anchor.Value.Token, logoBytes))
        {
            return OperationResult<bool>.Fail("domain_error", "Template is invalid: logo near first [ClientName] was not found.");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static (OpenXmlPart Part, Text Token)? FindFirstClientNameAnchor(WordprocessingDocument document)
    {
        foreach (var header in document.MainDocumentPart!.HeaderParts)
        {
            if (header.Header is null)
            {
                continue;
            }

            var token = header.Header.Descendants<Text>()
                .FirstOrDefault(t => (t.Text ?? string.Empty).Contains("[ClientName]", StringComparison.OrdinalIgnoreCase));
            if (token is not null)
            {
                return (header, token);
            }
        }

        var mainToken = document.MainDocumentPart.Document.Descendants<Text>()
            .FirstOrDefault(t => (t.Text ?? string.Empty).Contains("[ClientName]", StringComparison.OrdinalIgnoreCase));
        if (mainToken is not null)
        {
            return (document.MainDocumentPart, mainToken);
        }

        foreach (var footer in document.MainDocumentPart.FooterParts)
        {
            if (footer.Footer is null)
            {
                continue;
            }

            var token = footer.Footer.Descendants<Text>()
                .FirstOrDefault(t => (t.Text ?? string.Empty).Contains("[ClientName]", StringComparison.OrdinalIgnoreCase));
            if (token is not null)
            {
                return (footer, token);
            }
        }

        return null;
    }

    private static bool TryReplaceClosestImage(OpenXmlPart part, Text token, byte[] logoBytes)
    {
        var root = part switch
        {
            MainDocumentPart main => (OpenXmlElement?)main.Document,
            HeaderPart header => header.Header,
            FooterPart footer => footer.Footer,
            _ => null
        };

        if (root is null)
        {
            return false;
        }

        var drawings = root.Descendants<DocumentFormat.OpenXml.Wordprocessing.Drawing>()
            .Where(d => d.Descendants<A.Blip>().Any())
            .ToList();

        if (drawings.Count == 0)
        {
            return false;
        }

        var tokenParagraph = token.Ancestors<Paragraph>().FirstOrDefault();
        var tokenDrawing = token.Ancestors<DocumentFormat.OpenXml.Wordprocessing.Drawing>().FirstOrDefault();

        var ranked = RankLogoCandidates(drawings, tokenParagraph, tokenDrawing);
        var selected = ranked.FirstOrDefault() ?? drawings.First();
        var blip = selected.Descendants<A.Blip>().FirstOrDefault();
        if (blip?.Embed?.Value is null)
        {
            return false;
        }

        var relId = blip.Embed.Value;
        if (part.GetPartById(relId) is not ImagePart oldImage)
        {
            return false;
        }

        var (boxCx, boxCy) = GetCurrentImageBox(blip);
        if (!IsLikelyLogoBox(boxCx, boxCy))
        {
            // Defensive fallback against selecting background/cover images.
            var fallback = ranked.FirstOrDefault(d =>
            {
                var b = d.Descendants<A.Blip>().FirstOrDefault();
                if (b is null)
                {
                    return false;
                }

                var (cx, cy) = GetCurrentImageBox(b);
                return IsLikelyLogoBox(cx, cy);
            });

            if (fallback is not null)
            {
                selected = fallback;
                blip = selected.Descendants<A.Blip>().FirstOrDefault();
                if (blip?.Embed?.Value is null)
                {
                    return false;
                }

                relId = blip.Embed.Value;
                if (part.GetPartById(relId) is not ImagePart fallbackOldImage)
                {
                    return false;
                }

                oldImage = fallbackOldImage;
                (boxCx, boxCy) = GetCurrentImageBox(blip);
            }
        }
        part.DeletePart(oldImage);

        var contentType = ResolveImageContentType(logoBytes);
        ImagePart newImage = part switch
        {
            MainDocumentPart mp => mp.AddImagePart(contentType, relId),
            HeaderPart hp => hp.AddImagePart(contentType, relId),
            FooterPart fp => fp.AddImagePart(contentType, relId),
            _ => throw new InvalidOperationException("Unsupported document part for image replacement.")
        };
        using (var stream = new MemoryStream(logoBytes, writable: false))
        {
            newImage.FeedData(stream);
        }

        SetContainSize(blip, boxCx, boxCy, logoBytes);

        switch (root)
        {
            case Document d:
                d.Save();
                break;
            case Header h:
                h.Save();
                break;
            case Footer f:
                f.Save();
                break;
        }

        return true;
    }

    private static IEnumerable<DocumentFormat.OpenXml.Wordprocessing.Drawing> RankLogoCandidates(
        IReadOnlyList<DocumentFormat.OpenXml.Wordprocessing.Drawing> drawings,
        Paragraph? tokenParagraph,
        DocumentFormat.OpenXml.Wordprocessing.Drawing? tokenDrawing)
    {
        var scored = new List<(DocumentFormat.OpenXml.Wordprocessing.Drawing Drawing, int Score)>();

        for (var i = 0; i < drawings.Count; i++)
        {
            var d = drawings[i];
            var blip = d.Descendants<A.Blip>().FirstOrDefault();
            if (blip is null)
            {
                continue;
            }

            var (cx, cy) = GetCurrentImageBox(blip);
            var score = 0;

            if (IsLikelyLogoBox(cx, cy))
            {
                score += 100;
            }

            if (cy > 0)
            {
                var ratio = (double)cx / cy;
                if (ratio >= 1.3 && ratio <= 7.0)
                {
                    score += 30;
                }
            }

            var area = cx * cy;
            if (area <= 2_500_000_000_000L)
            {
                score += 20;
            }
            else
            {
                score -= 40;
            }

            if (tokenDrawing is not null)
            {
                var tokenIndex = -1;
                for (var j = 0; j < drawings.Count; j++)
                {
                    if (drawings[j] == tokenDrawing)
                    {
                        tokenIndex = j;
                        break;
                    }
                }

                var dist = tokenIndex >= 0 ? Math.Abs(tokenIndex - i) : 50;
                score += Math.Max(0, 20 - dist);
            }
            else if (tokenParagraph is not null)
            {
                var hasSameParagraph = tokenParagraph.Descendants<DocumentFormat.OpenXml.Wordprocessing.Drawing>().Any(x => x == d);
                if (hasSameParagraph)
                {
                    score += 15;
                }
            }

            scored.Add((d, score));
        }

        return scored.OrderByDescending(x => x.Score).Select(x => x.Drawing);
    }

    private static bool IsLikelyLogoBox(long cx, long cy)
    {
        if (cx <= 0 || cy <= 0)
        {
            return false;
        }

        var ratio = (double)cx / cy;
        var area = cx * cy;

        // Reject very large images (usually page backgrounds/covers).
        if (area > 6_500_000_000_000L)
        {
            return false;
        }

        // Logo is generally horizontal/compact, not full-page.
        return ratio >= 1.1 && ratio <= 8.0;
    }

    private static (long Cx, long Cy) GetCurrentImageBox(A.Blip blip)
    {
        var drawing = blip.Ancestors<DocumentFormat.OpenXml.Wordprocessing.Drawing>().FirstOrDefault();
        if (drawing?.Inline?.Extent is not null)
        {
            return (drawing.Inline.Extent.Cx ?? 2000000L, drawing.Inline.Extent.Cy ?? 700000L);
        }

        if (drawing?.Anchor?.Extent is not null)
        {
            return (drawing.Anchor.Extent.Cx ?? 2000000L, drawing.Anchor.Extent.Cy ?? 700000L);
        }

        var ext = drawing?.Descendants<A.Extents>().FirstOrDefault();
        return ext is null ? (2000000L, 700000L) : (ext.Cx ?? 2000000L, ext.Cy ?? 700000L);
    }

    private static void SetContainSize(A.Blip blip, long boxCx, long boxCy, byte[] imageBytes)
    {
        if (boxCx <= 0 || boxCy <= 0)
        {
            return;
        }

        var info = Image.Identify(imageBytes);
        if (info is null || info.Width <= 0 || info.Height <= 0)
        {
            return;
        }

        var dpiX = info.Metadata.HorizontalResolution > 0 ? info.Metadata.HorizontalResolution : DefaultDpi;
        var dpiY = info.Metadata.VerticalResolution > 0 ? info.Metadata.VerticalResolution : DefaultDpi;

        var imageCx = (long)Math.Round(info.Width / dpiX * EmuPerInch);
        var imageCy = (long)Math.Round(info.Height / dpiY * EmuPerInch);
        if (imageCx <= 0 || imageCy <= 0)
        {
            return;
        }

        var sourceRatio = (double)info.Width / info.Height;
        var isSquareLike = sourceRatio >= 0.85 && sourceRatio <= 1.25;

        // Fixed fit area requested:
        // - General max area: 5cm x 3cm
        // - Square-like logos: try 3cm x 3cm
        var maxWidth = isSquareLike ? 3d * EmuPerCm : 5d * EmuPerCm;
        var maxHeight = 3d * EmuPerCm;

        var scale = Math.Min(maxWidth / imageCx, maxHeight / imageCy);
        if (scale <= 0)
        {
            return;
        }

        var newCx = (long)Math.Round(imageCx * scale);
        var newCy = (long)Math.Round(imageCy * scale);

        // Final safety fit in case numeric rounding pushes dimensions past limits.
        if (newCx > maxWidth || newCy > maxHeight)
        {
            var correction = Math.Min(maxWidth / newCx, maxHeight / newCy);
            newCx = (long)Math.Round(newCx * correction);
            newCy = (long)Math.Round(newCy * correction);
        }

        var drawing = blip.Ancestors<DocumentFormat.OpenXml.Wordprocessing.Drawing>().FirstOrDefault();
        if (drawing?.Inline?.Extent is not null)
        {
            drawing.Inline.Extent.Cx = newCx;
            drawing.Inline.Extent.Cy = newCy;
        }

        if (drawing?.Anchor?.Extent is not null)
        {
            drawing.Anchor.Extent.Cx = newCx;
            drawing.Anchor.Extent.Cy = newCy;
        }

        foreach (var ext in drawing?.Descendants<A.Extents>() ?? Enumerable.Empty<A.Extents>())
        {
            ext.Cx = newCx;
            ext.Cy = newCy;
        }

    }

    private static string ResolveImageContentType(byte[] data)
    {
        if (data.Length >= 12
            && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46
            && data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
        {
            return "image/webp";
        }

        if (data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
        {
            return "image/png";
        }

        if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xD8)
        {
            return "image/jpeg";
        }

        return "image/png";
    }

    private async Task<OperationResult<bool>> ConvertDocxToPdfAsync(string inputDocxPath, string outputDir, CancellationToken cancellationToken)
    {
        // Prefer Word automation when available to guarantee TOC/field refresh before PDF.
        if (ResolveWordPath() is not null)
        {
            return await ConvertWithWordAutomationAsync(inputDocxPath, outputDir, cancellationToken);
        }

        var sofficePath = ResolveSofficePath();
        if (!string.IsNullOrWhiteSpace(sofficePath))
        {
            return await ConvertWithLibreOfficeAsync(sofficePath, inputDocxPath, outputDir, cancellationToken);
        }

        return OperationResult<bool>.Fail("domain_error", "No PDF converter found. Install LibreOffice or Microsoft Word.");
    }

    private async Task<OperationResult<bool>> ConvertWithLibreOfficeAsync(string sofficePath, string inputDocxPath, string outputDir, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = sofficePath,
            Arguments = $"--headless --convert-to pdf --outdir \"{outputDir}\" \"{inputDocxPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = new Process { StartInfo = startInfo };
            if (!process.Start())
            {
                return OperationResult<bool>.Fail("domain_error", "Unable to start PDF converter process.");
            }

            var timeout = GetTimeoutSeconds(45);
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await process.WaitForExitAsync(linkedCts.Token);
            var stdOut = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stdErr = await process.StandardError.ReadToEndAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                return OperationResult<bool>.Fail("domain_error", $"DOCX to PDF conversion failed: {PickMessage(stdOut, stdErr)}");
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<bool>.Fail("domain_error", "DOCX to PDF conversion timed out.");
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Fail("domain_error", $"PDF converter error: {ex.GetBaseException().Message}");
        }
    }

    private async Task<OperationResult<bool>> ConvertWithWordAutomationAsync(string inputDocxPath, string outputDir, CancellationToken cancellationToken)
    {
        var outputPdfPath = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(inputDocxPath)}.pdf");
        var escapedInput = inputDocxPath.Replace("'", "''");
        var escapedOutput = outputPdfPath.Replace("'", "''");

        var script = string.Join("; ", new[]
        {
            "$ErrorActionPreference = 'Stop'",
            "$word = New-Object -ComObject Word.Application",
            "$word.Visible = $false",
            "$doc = $word.Documents.Open('" + escapedInput + "', $false, $true)",
            "$doc.Repaginate()",
            "$doc.TablesOfContents | ForEach-Object { $_.Update() }",
            "$null = $doc.Fields.Update()",
            "$range = $doc.StoryRanges",
            "while ($range -ne $null) { $null = $range.Fields.Update(); $range = $range.NextStoryRange }",
            "$doc.SaveAs([ref]'" + escapedOutput + "', [ref]17)",
            "$doc.Close()",
            "$word.Quit()"
        });

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{script}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = new Process { StartInfo = startInfo };
            if (!process.Start())
            {
                return OperationResult<bool>.Fail("domain_error", "Unable to start Word PDF converter process.");
            }

            var timeout = GetTimeoutSeconds(60);
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await process.WaitForExitAsync(linkedCts.Token);
            var stdOut = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stdErr = await process.StandardError.ReadToEndAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                return OperationResult<bool>.Fail("domain_error", $"Word PDF conversion failed: {PickMessage(stdOut, stdErr)}");
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<bool>.Fail("domain_error", "Word PDF conversion timed out.");
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Fail("domain_error", $"Word converter error: {ex.GetBaseException().Message}");
        }
    }

    private int GetTimeoutSeconds(int defaultSeconds)
        => int.TryParse(_configuration["DocumentExport:TimeoutSeconds"], out var value) && value > 0 ? value : defaultSeconds;

    private static string PickMessage(string stdOut, string stdErr)
        => string.IsNullOrWhiteSpace(stdErr) ? stdOut.Trim() : stdErr.Trim();

    private string? ResolveSofficePath()
    {
        var configured = _configuration["DocumentExport:LibreOfficePath"]?.Trim();
        if (!string.IsNullOrWhiteSpace(configured))
        {
            if (File.Exists(configured))
            {
                return configured;
            }

            if (string.Equals(configured, "soffice", StringComparison.OrdinalIgnoreCase) && CommandExists("soffice"))
            {
                return "soffice";
            }
        }

        var candidates = new[]
        {
            @"C:\Program Files\LibreOffice\program\soffice.exe",
            @"C:\Program Files (x86)\LibreOffice\program\soffice.exe"
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string? ResolveWordPath()
    {
        var candidates = new[]
        {
            @"C:\Program Files\Microsoft Office\root\Office16\WINWORD.EXE",
            @"C:\Program Files (x86)\Microsoft Office\root\Office16\WINWORD.EXE"
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static bool CommandExists(string command)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        foreach (var dir in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var fullPath = Path.Combine(dir, command);
            if (File.Exists(fullPath) || File.Exists(fullPath + ".exe"))
            {
                return true;
            }
        }

        return false;
    }
}
