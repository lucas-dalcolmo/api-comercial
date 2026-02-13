using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;

namespace Api.Comercial.Services;

public interface IBenefitFormulaVariableService
{
    Task<OperationResult<BenefitFormulaVariableDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<BenefitFormulaVariableDto>>> GetAllAsync(BenefitFormulaVariableQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<BenefitFormulaVariableDto>> CreateAsync(BenefitFormulaVariableCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<BenefitFormulaVariableDto>> PatchAsync(int id, BenefitFormulaVariableUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<BenefitFormulaVariableDto>> ReactivateAsync(int id, CancellationToken cancellationToken);
}

public sealed class BenefitFormulaVariableService : IBenefitFormulaVariableService
{
    private static readonly HashSet<string> AllowedScopes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Contract",
        "EmployeeFromContract"
    };

    private readonly ApeironDbContext _context;

    public BenefitFormulaVariableService(ApeironDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<BenefitFormulaVariableDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.BenefitFormulaVariables.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<BenefitFormulaVariableDto>.Fail("not_found", "Record not found.");
        }

        return OperationResult<BenefitFormulaVariableDto>.Ok(Map(entity));
    }

    public async Task<OperationResult<PagedResult<BenefitFormulaVariableDto>>> GetAllAsync(BenefitFormulaVariableQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dataQuery = _context.BenefitFormulaVariables.AsNoTracking().AsQueryable();
        if (query.Id.HasValue) dataQuery = dataQuery.Where(v => v.Id == query.Id.Value);
        if (!string.IsNullOrWhiteSpace(query.VariableKey)) dataQuery = dataQuery.Where(v => v.VariableKey == query.VariableKey.Trim());
        if (!string.IsNullOrWhiteSpace(query.SourceScope)) dataQuery = dataQuery.Where(v => v.SourceScope == query.SourceScope.Trim());
        if (!string.IsNullOrWhiteSpace(query.SourceSchema)) dataQuery = dataQuery.Where(v => v.SourceSchema == query.SourceSchema.Trim());
        if (!string.IsNullOrWhiteSpace(query.SourceTable)) dataQuery = dataQuery.Where(v => v.SourceTable == query.SourceTable.Trim());
        if (!string.IsNullOrWhiteSpace(query.SourceColumn)) dataQuery = dataQuery.Where(v => v.SourceColumn == query.SourceColumn.Trim());
        if (query.Active.HasValue) dataQuery = dataQuery.Where(v => v.Active == query.Active.Value);
        else dataQuery = dataQuery.Where(v => v.Active);

        var totalCount = await dataQuery.CountAsync(cancellationToken);
        var items = await dataQuery
            .OrderBy(v => v.Id)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(v => Map(v))
            .ToListAsync(cancellationToken);

        return OperationResult<PagedResult<BenefitFormulaVariableDto>>.Ok(new PagedResult<BenefitFormulaVariableDto>(items, currentPage, pageSize, totalCount));
    }

    public async Task<OperationResult<BenefitFormulaVariableDto>> CreateAsync(BenefitFormulaVariableCreateDto dto, CancellationToken cancellationToken)
    {
        var normalized = NormalizeAndValidate(dto.VariableKey, dto.SourceScope, dto.SourceSchema, dto.SourceTable, dto.SourceColumn);
        if (!normalized.Success)
        {
            return OperationResult<BenefitFormulaVariableDto>.Fail("validation", normalized.ErrorMessage!);
        }

        var exists = await _context.BenefitFormulaVariables
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VariableKey == normalized.VariableKey!, cancellationToken);
        if (exists is not null)
        {
            var message = exists.Active
                ? "VariableKey already exists."
                : "VariableKey already exists as an inactive record.";
            return OperationResult<BenefitFormulaVariableDto>.Fail("conflict", message);
        }

        var entity = new BenefitFormulaVariable
        {
            VariableKey = normalized.VariableKey!,
            SourceScope = normalized.SourceScope!,
            SourceSchema = normalized.SourceSchema!,
            SourceTable = normalized.SourceTable!,
            SourceColumn = normalized.SourceColumn!,
            Active = true
        };

        _context.BenefitFormulaVariables.Add(entity);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<BenefitFormulaVariableDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<BenefitFormulaVariableDto>.Ok(Map(entity));
    }

    public async Task<OperationResult<BenefitFormulaVariableDto>> PatchAsync(int id, BenefitFormulaVariableUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.BenefitFormulaVariables.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<BenefitFormulaVariableDto>.Fail("not_found", "Record not found.");
        }

        if (dto.VariableKey is null
            && dto.SourceScope is null
            && dto.SourceSchema is null
            && dto.SourceTable is null
            && dto.SourceColumn is null
            && !dto.Active.HasValue)
        {
            return OperationResult<BenefitFormulaVariableDto>.Fail("validation", "At least one field must be provided.");
        }

        var candidate = NormalizeAndValidate(
            dto.VariableKey ?? entity.VariableKey,
            dto.SourceScope ?? entity.SourceScope,
            dto.SourceSchema ?? entity.SourceSchema,
            dto.SourceTable ?? entity.SourceTable,
            dto.SourceColumn ?? entity.SourceColumn);
        if (!candidate.Success)
        {
            return OperationResult<BenefitFormulaVariableDto>.Fail("validation", candidate.ErrorMessage!);
        }

        var keyChanged = !string.Equals(entity.VariableKey, candidate.VariableKey, StringComparison.Ordinal);
        if (keyChanged)
        {
            var exists = await _context.BenefitFormulaVariables
                .AsNoTracking()
                .AnyAsync(v => v.VariableKey == candidate.VariableKey! && v.Id != id, cancellationToken);
            if (exists)
            {
                return OperationResult<BenefitFormulaVariableDto>.Fail("conflict", "VariableKey already exists.");
            }
        }

        entity.VariableKey = candidate.VariableKey!;
        entity.SourceScope = candidate.SourceScope!;
        entity.SourceSchema = candidate.SourceSchema!;
        entity.SourceTable = candidate.SourceTable!;
        entity.SourceColumn = candidate.SourceColumn!;
        if (dto.Active.HasValue) entity.Active = dto.Active.Value;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<BenefitFormulaVariableDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<BenefitFormulaVariableDto>.Ok(Map(entity));
    }

    public async Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.BenefitFormulaVariables.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
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

    public async Task<OperationResult<BenefitFormulaVariableDto>> ReactivateAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.BenefitFormulaVariables.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (entity is null)
        {
            return OperationResult<BenefitFormulaVariableDto>.Fail("not_found", "Record not found.");
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
                return OperationResult<BenefitFormulaVariableDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
            }
        }

        return OperationResult<BenefitFormulaVariableDto>.Ok(Map(entity));
    }

    private static (bool Success, string? ErrorMessage, string? VariableKey, string? SourceScope, string? SourceSchema, string? SourceTable, string? SourceColumn)
        NormalizeAndValidate(string variableKey, string sourceScope, string sourceSchema, string sourceTable, string sourceColumn)
    {
        var key = variableKey.Trim();
        var scope = sourceScope.Trim();
        var schema = sourceSchema.Trim();
        var table = sourceTable.Trim();
        var column = sourceColumn.Trim();

        if (string.IsNullOrWhiteSpace(key))
        {
            return (false, "VariableKey is required.", null, null, null, null, null);
        }

        if (key.Contains('[') || key.Contains(']'))
        {
            return (false, "VariableKey must not include brackets. Use only the inner key, e.g., EmployeeContract.BaseSalaryUsd.", null, null, null, null, null);
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(key, "^[A-Za-z0-9_.]+$"))
        {
            return (false, "VariableKey contains invalid characters.", null, null, null, null, null);
        }

        if (!AllowedScopes.Contains(scope))
        {
            return (false, "SourceScope must be one of: Contract, EmployeeFromContract.", null, null, null, null, null);
        }

        if (!IsSqlIdentifier(schema) || !IsSqlIdentifier(table) || !IsSqlIdentifier(column))
        {
            return (false, "SourceSchema, SourceTable and SourceColumn must use SQL identifier format [A-Za-z0-9_].", null, null, null, null, null);
        }

        return (true, null, key, scope, schema, table, column);
    }

    private static bool IsSqlIdentifier(string value)
        => !string.IsNullOrWhiteSpace(value)
           && System.Text.RegularExpressions.Regex.IsMatch(value, "^[A-Za-z_][A-Za-z0-9_]*$");

    private static BenefitFormulaVariableDto Map(BenefitFormulaVariable entity)
        => new(
            entity.Id,
            entity.VariableKey,
            entity.SourceScope,
            entity.SourceSchema,
            entity.SourceTable,
            entity.SourceColumn,
            entity.Active);

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
