using System.Text.RegularExpressions;

namespace Api.Comercial.Services;

public interface IHtmlSanitizerService
{
    string Sanitize(string? html);
}

public sealed class HtmlSanitizerService : IHtmlSanitizerService
{
    private static readonly Regex ScriptLikeRegex = new(
        "<(script|style|iframe|object|embed|form|meta|link)[^>]*?>[\\s\\S]*?<\\s*/\\s*\\1\\s*>|<(script|style|iframe|object|embed|form|meta|link)[^>]*/?>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EventAttributeRegex = new(
        "\\s+on[a-z]+\\s*=\\s*(['\"]).*?\\1|\\s+on[a-z]+\\s*=\\s*[^\\s>]+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex JsProtocolRegex = new(
        "(href|src)\\s*=\\s*(['\"])\\s*javascript:[^'\"]*\\2",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var sanitized = ScriptLikeRegex.Replace(html, string.Empty);
        sanitized = EventAttributeRegex.Replace(sanitized, string.Empty);
        sanitized = JsProtocolRegex.Replace(sanitized, "$1=\"#\"");

        return sanitized.Trim();
    }
}
