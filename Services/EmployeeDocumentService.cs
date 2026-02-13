using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;

namespace Api.Comercial.Services;

public interface IEmployeeDocumentService
{
    Task<OperationResult<EmployeeDocumentDto>> GetByIdAsync(int employeeId, int documentId, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<EmployeeDocumentDto>>> GetAllAsync(EmployeeDocumentQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeDocumentDto>> CreateAsync(int employeeId, EmployeeDocumentCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeDocumentDto>> PatchAsync(int employeeId, int documentId, EmployeeDocumentUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(int employeeId, int documentId, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeDocumentDto>> ReactivateAsync(int employeeId, int documentId, CancellationToken cancellationToken);
}

public sealed class EmployeeDocumentService : IEmployeeDocumentService
{
    private readonly ApeironDbContext _context;

    public EmployeeDocumentService(ApeironDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<EmployeeDocumentDto>> GetByIdAsync(int employeeId, int documentId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.EmployeeId == employeeId && d.Id == documentId, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeDocumentDto>.Fail("not_found", "Record not found.");
        }

        return OperationResult<EmployeeDocumentDto>.Ok(MapDocument(entity));
    }

    public async Task<OperationResult<PagedResult<EmployeeDocumentDto>>> GetAllAsync(EmployeeDocumentQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dataQuery = _context.EmployeeDocuments.AsNoTracking().AsQueryable();

        if (query.EmployeeId.HasValue)
        {
            dataQuery = dataQuery.Where(e => e.EmployeeId == query.EmployeeId.Value);
        }

        if (query.DocumentTypeId.HasValue)
        {
            dataQuery = dataQuery.Where(e => e.DocumentTypeId == query.DocumentTypeId.Value);
        }

        if (query.Active.HasValue)
        {
            dataQuery = dataQuery.Where(e => e.Active == query.Active.Value);
        }
        else
        {
            dataQuery = dataQuery.Where(e => e.Active);
        }

        var totalCount = await dataQuery.CountAsync(cancellationToken);
        var items = await dataQuery
            .OrderByDescending(e => e.IssueDate)
            .ThenByDescending(e => e.Id)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(e => MapDocument(e))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<EmployeeDocumentDto>(items, currentPage, pageSize, totalCount);
        return OperationResult<PagedResult<EmployeeDocumentDto>>.Ok(result);
    }

    public async Task<OperationResult<EmployeeDocumentDto>> CreateAsync(int employeeId, EmployeeDocumentCreateDto dto, CancellationToken cancellationToken)
    {
        if (!IsValidCountryCode(dto.CountryCode))
        {
            return OperationResult<EmployeeDocumentDto>.Fail("validation", "Country code must be ISO-3166 alpha-2.");
        }

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
        if (employee is null || !employee.Active)
        {
            return OperationResult<EmployeeDocumentDto>.Fail("not_found", "Employee not found.");
        }

        var entity = new EmployeeDocument
        {
            EmployeeId = employeeId,
            DocumentTypeId = dto.DocumentTypeId,
            DocumentNumber = dto.DocumentNumber?.Trim(),
            CountryCode = NormalizeCountryCode(dto.CountryCode),
            IssueDate = dto.IssueDate,
            ExpiryDate = dto.ExpiryDate,
            Notes = dto.Notes?.Trim(),
            Active = true
        };

        _context.EmployeeDocuments.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeDocumentDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<EmployeeDocumentDto>.Ok(MapDocument(entity));
    }

    public async Task<OperationResult<EmployeeDocumentDto>> PatchAsync(int employeeId, int documentId, EmployeeDocumentUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeDocuments
            .FirstOrDefaultAsync(d => d.EmployeeId == employeeId && d.Id == documentId, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeDocumentDto>.Fail("not_found", "Record not found.");
        }

        if (dto.DocumentTypeId is null
            && dto.DocumentNumber is null
            && dto.CountryCode is null
            && dto.IssueDate is null
            && dto.ExpiryDate is null
            && dto.Notes is null
            && !dto.Active.HasValue)
        {
            return OperationResult<EmployeeDocumentDto>.Fail("validation", "At least one field must be provided.");
        }

        if (dto.CountryCode is not null && !IsValidCountryCode(dto.CountryCode))
        {
            return OperationResult<EmployeeDocumentDto>.Fail("validation", "Country code must be ISO-3166 alpha-2.");
        }

        if (dto.DocumentTypeId.HasValue) entity.DocumentTypeId = dto.DocumentTypeId.Value;
        if (dto.DocumentNumber is not null) entity.DocumentNumber = dto.DocumentNumber.Trim();
        if (dto.CountryCode is not null) entity.CountryCode = NormalizeCountryCode(dto.CountryCode);
        if (dto.IssueDate.HasValue) entity.IssueDate = dto.IssueDate.Value;
        if (dto.ExpiryDate.HasValue) entity.ExpiryDate = dto.ExpiryDate.Value;
        if (dto.Notes is not null) entity.Notes = dto.Notes.Trim();
        if (dto.Active.HasValue) entity.Active = dto.Active.Value;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeDocumentDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<EmployeeDocumentDto>.Ok(MapDocument(entity));
    }

    public async Task<OperationResult<bool>> DeleteAsync(int employeeId, int documentId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeDocuments
            .FirstOrDefaultAsync(d => d.EmployeeId == employeeId && d.Id == documentId, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<bool>.Fail("not_found", "Record not found.");
        }

        entity.Active = false;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<bool>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<bool>.Ok(true);
    }

    public async Task<OperationResult<EmployeeDocumentDto>> ReactivateAsync(int employeeId, int documentId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeDocuments
            .FirstOrDefaultAsync(d => d.EmployeeId == employeeId && d.Id == documentId, cancellationToken);

        if (entity is null)
        {
            return OperationResult<EmployeeDocumentDto>.Fail("not_found", "Record not found.");
        }

        if (!entity.Active)
        {
            entity.Active = true;
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                return OperationResult<EmployeeDocumentDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
            }
        }

        return OperationResult<EmployeeDocumentDto>.Ok(MapDocument(entity));
    }

    private static EmployeeDocumentDto MapDocument(EmployeeDocument entity)
        => new(
            entity.Id,
            entity.EmployeeId,
            entity.DocumentTypeId,
            entity.DocumentNumber,
            entity.CountryCode,
            entity.IssueDate,
            entity.ExpiryDate,
            entity.Notes,
            entity.Active);

    private static string? NormalizeCountryCode(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return null;
        }

        return countryCode.Trim().ToUpperInvariant();
    }

    private static bool IsValidCountryCode(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return true;
        }

        return countryCode.Trim().Length == 2;
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
