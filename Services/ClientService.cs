using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;

namespace Api.Comercial.Services;

public interface IClientService
{
    Task<OperationResult<ClientDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<ClientDto>>> GetAllAsync(ClientQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<ClientDto>> CreateAsync(ClientCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<ClientDto>> PatchAsync(int id, ClientUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<ClientDto>> UploadLogoAsync(int id, IFormFile? file, CancellationToken cancellationToken);
    Task<OperationResult<ClientDto>> RemoveLogoAsync(int id, CancellationToken cancellationToken);
}

public sealed class ClientService : IClientService
{
    private const int NameMaxLength = 200;
    private const int LegalNameMaxLength = 300;
    private const long MaxLogoSizeBytes = 2 * 1024 * 1024;

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp"
    };

    private readonly ApeironDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ClientService(ApeironDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<OperationResult<ClientDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<ClientDto>.Fail("not_found", "Record not found.");
        }

        return OperationResult<ClientDto>.Ok(Map(entity));
    }

    public async Task<OperationResult<PagedResult<ClientDto>>> GetAllAsync(ClientQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);
        var name = query.Name?.Trim();

        var dataQuery = _context.Clients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            dataQuery = dataQuery.Where(c => c.Name.Contains(name));
        }

        if (query.Active.HasValue)
        {
            dataQuery = dataQuery.Where(c => c.Active == query.Active.Value);
        }
        else
        {
            dataQuery = dataQuery.Where(c => c.Active);
        }

        var totalCount = await dataQuery.CountAsync(cancellationToken);
        var items = await dataQuery
            .OrderBy(c => c.Name)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClientDto(c.Id, c.Name, c.LegalName, c.LogoUrl, c.Active))
            .ToListAsync(cancellationToken);

        return OperationResult<PagedResult<ClientDto>>.Ok(new PagedResult<ClientDto>(items, currentPage, pageSize, totalCount));
    }

    public async Task<OperationResult<ClientDto>> CreateAsync(ClientCreateDto dto, CancellationToken cancellationToken)
    {
        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<ClientDto>.Fail("validation", "Name is required.");
        }

        if (name.Length > NameMaxLength)
        {
            return OperationResult<ClientDto>.Fail("validation", $"Name must be at most {NameMaxLength} characters.");
        }

        var legalName = string.IsNullOrWhiteSpace(dto.LegalName) ? null : dto.LegalName.Trim();
        if (legalName is not null && legalName.Length > LegalNameMaxLength)
        {
            return OperationResult<ClientDto>.Fail("validation", $"LegalName must be at most {LegalNameMaxLength} characters.");
        }

        var entity = new Client
        {
            Id = await GetNextClientIdAsync(cancellationToken),
            Name = name,
            LegalName = legalName,
            Active = true
        };

        _context.Clients.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<ClientDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<ClientDto>.Ok(Map(entity));
    }

    public async Task<OperationResult<ClientDto>> PatchAsync(int id, ClientUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<ClientDto>.Fail("not_found", "Record not found.");
        }

        if (dto.Name is null && dto.LegalName is null && !dto.Active.HasValue)
        {
            return OperationResult<ClientDto>.Fail("validation", "At least one field must be provided.");
        }

        if (dto.Name is not null)
        {
            var name = dto.Name.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return OperationResult<ClientDto>.Fail("validation", "Name is required.");
            }

            if (name.Length > NameMaxLength)
            {
                return OperationResult<ClientDto>.Fail("validation", $"Name must be at most {NameMaxLength} characters.");
            }

            entity.Name = name;
        }

        if (dto.LegalName is not null)
        {
            var legalName = dto.LegalName.Trim();
            if (legalName.Length > LegalNameMaxLength)
            {
                return OperationResult<ClientDto>.Fail("validation", $"LegalName must be at most {LegalNameMaxLength} characters.");
            }

            entity.LegalName = string.IsNullOrWhiteSpace(legalName) ? null : legalName;
        }

        if (dto.Active.HasValue)
        {
            entity.Active = dto.Active.Value;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<ClientDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<ClientDto>.Ok(Map(entity));
    }

    public async Task<OperationResult<ClientDto>> UploadLogoAsync(int id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return OperationResult<ClientDto>.Fail("validation", "Logo file is required.");
        }

        if (file.Length <= 0)
        {
            return OperationResult<ClientDto>.Fail("validation", "Logo file cannot be empty.");
        }

        if (file.Length > MaxLogoSizeBytes)
        {
            return OperationResult<ClientDto>.Fail("validation", "Logo file size must be at most 2MB.");
        }

        var contentType = file.ContentType?.Trim();
        if (string.IsNullOrWhiteSpace(contentType) || !AllowedMimeTypes.Contains(contentType))
        {
            return OperationResult<ClientDto>.Fail("validation", "Unsupported logo content type. Use png, jpg or webp.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return OperationResult<ClientDto>.Fail("validation", "Unsupported logo file extension. Use .png, .jpg, .jpeg or .webp.");
        }

        var entity = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<ClientDto>.Fail("not_found", "Record not found.");
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var logosDir = Path.Combine(webRootPath, "uploads", "client-logos");
        Directory.CreateDirectory(logosDir);

        var safeExtension = extension.ToLowerInvariant();
        var fileName = $"client_{id}_{Guid.NewGuid():N}{safeExtension}";
        var filePath = Path.Combine(logosDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        DeleteLogoIfLocal(entity.LogoUrl, webRootPath);

        entity.LogoUrl = $"/uploads/client-logos/{fileName}";

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<ClientDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<ClientDto>.Ok(Map(entity));
    }

    public async Task<OperationResult<ClientDto>> RemoveLogoAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<ClientDto>.Fail("not_found", "Record not found.");
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        DeleteLogoIfLocal(entity.LogoUrl, webRootPath);
        entity.LogoUrl = null;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<ClientDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<ClientDto>.Ok(Map(entity));
    }

    private static void DeleteLogoIfLocal(string? logoUrl, string webRootPath)
    {
        if (string.IsNullOrWhiteSpace(logoUrl) || !logoUrl.StartsWith("/uploads/client-logos/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relative = logoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(webRootPath, relative);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private static ClientDto Map(Client entity)
        => new(entity.Id, entity.Name, entity.LegalName, entity.LogoUrl, entity.Active);

    private async Task<int> GetNextClientIdAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.Clients
            .AsNoTracking()
            .Select(c => (int?)c.Id)
            .MaxAsync(cancellationToken);
        return (maxId ?? 0) + 1;
    }

    private static int NormalizePage(int? page)
        => page.HasValue && page.Value > 0 ? page.Value : 1;

    private static int NormalizePageSize(int? pageSize)
    {
        if (!pageSize.HasValue || pageSize.Value <= 0)
        {
            return 50;
        }

        return pageSize.Value > 200 ? 200 : pageSize.Value;
    }
}
