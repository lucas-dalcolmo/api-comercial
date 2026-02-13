using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;

namespace Api.Comercial.Services;

public interface IScoreCategoryWeightService
{
    Task<OperationResult<ScoreCategoryWeightDto>> GetByIdAsync(string categoryName, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<ScoreCategoryWeightDto>>> GetAllAsync(ScoreCategoryWeightQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<ScoreCategoryWeightDto>> CreateAsync(ScoreCategoryWeightCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<ScoreCategoryWeightDto>> PatchAsync(string categoryName, ScoreCategoryWeightUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(string categoryName, CancellationToken cancellationToken);
    Task<OperationResult<ScoreCategoryWeightDto>> ReactivateAsync(string categoryName, CancellationToken cancellationToken);
}

public sealed class ScoreCategoryWeightService : IScoreCategoryWeightService
{
    private const int CategoryNameMaxLength = 60;
    private readonly ApeironDbContext _context;

    public ScoreCategoryWeightService(ApeironDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<ScoreCategoryWeightDto>> GetByIdAsync(string categoryName, CancellationToken cancellationToken)
    {
        var normalized = NormalizeName(categoryName);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return OperationResult<ScoreCategoryWeightDto>.Fail("validation", "CategoryName is required.");
        }

        var entity = await _context.ScoreCategoryWeights.FindAsync(new object[] { normalized }, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<ScoreCategoryWeightDto>.Fail("not_found", "Record not found.");
        }

        var result = new ScoreCategoryWeightDto(entity.CategoryName, entity.CategoryWeight, entity.Active);
        return OperationResult<ScoreCategoryWeightDto>.Ok(result);
    }

    public async Task<OperationResult<PagedResult<ScoreCategoryWeightDto>>> GetAllAsync(ScoreCategoryWeightQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);
        var name = NormalizeName(query.CategoryName);

        var dataQuery = _context.ScoreCategoryWeights.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            dataQuery = dataQuery.Where(e => e.CategoryName == name);
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
            .OrderBy(e => e.CategoryName)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ScoreCategoryWeightDto(e.CategoryName, e.CategoryWeight, e.Active))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ScoreCategoryWeightDto>(items, currentPage, pageSize, totalCount);
        return OperationResult<PagedResult<ScoreCategoryWeightDto>>.Ok(result);
    }

    public async Task<OperationResult<ScoreCategoryWeightDto>> CreateAsync(ScoreCategoryWeightCreateDto dto, CancellationToken cancellationToken)
    {
        var categoryName = NormalizeName(dto.CategoryName);
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return OperationResult<ScoreCategoryWeightDto>.Fail("validation", "CategoryName is required.");
        }

        if (categoryName.Length > CategoryNameMaxLength)
        {
            return OperationResult<ScoreCategoryWeightDto>.Fail("validation", $"CategoryName must be at most {CategoryNameMaxLength} characters.");
        }

        if (dto.CategoryWeight < 0)
        {
            return OperationResult<ScoreCategoryWeightDto>.Fail("validation", "CategoryWeight must be greater than or equal to 0.");
        }

        var existing = await _context.ScoreCategoryWeights.FindAsync(new object[] { categoryName }, cancellationToken);
        if (existing is not null)
        {
            var message = existing.Active
                ? "CategoryName already exists."
                : "CategoryName already exists as an inactive record.";
            return OperationResult<ScoreCategoryWeightDto>.Fail("conflict", message);
        }

        var entity = new ScoreCategoryWeight
        {
            CategoryName = categoryName,
            CategoryWeight = dto.CategoryWeight,
            Active = true
        };

        _context.ScoreCategoryWeights.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<ScoreCategoryWeightDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var result = new ScoreCategoryWeightDto(entity.CategoryName, entity.CategoryWeight, entity.Active);
        return OperationResult<ScoreCategoryWeightDto>.Ok(result);
    }

    public async Task<OperationResult<ScoreCategoryWeightDto>> PatchAsync(string categoryName, ScoreCategoryWeightUpdateDto dto, CancellationToken cancellationToken)
    {
        var normalized = NormalizeName(categoryName);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return OperationResult<ScoreCategoryWeightDto>.Fail("validation", "CategoryName is required.");
        }

        var entity = await _context.ScoreCategoryWeights.FindAsync(new object[] { normalized }, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<ScoreCategoryWeightDto>.Fail("not_found", "Record not found.");
        }

        if (!dto.CategoryWeight.HasValue && !dto.Active.HasValue)
        {
            return OperationResult<ScoreCategoryWeightDto>.Fail("validation", "At least one field must be provided.");
        }

        if (dto.CategoryWeight.HasValue)
        {
            if (dto.CategoryWeight.Value < 0)
            {
                return OperationResult<ScoreCategoryWeightDto>.Fail("validation", "CategoryWeight must be greater than or equal to 0.");
            }

            entity.CategoryWeight = dto.CategoryWeight.Value;
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
            return OperationResult<ScoreCategoryWeightDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var result = new ScoreCategoryWeightDto(entity.CategoryName, entity.CategoryWeight, entity.Active);
        return OperationResult<ScoreCategoryWeightDto>.Ok(result);
    }

    public async Task<OperationResult<bool>> DeleteAsync(string categoryName, CancellationToken cancellationToken)
    {
        var normalized = NormalizeName(categoryName);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return OperationResult<bool>.Fail("validation", "CategoryName is required.");
        }

        var entity = await _context.ScoreCategoryWeights.FindAsync(new object[] { normalized }, cancellationToken);
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

    public async Task<OperationResult<ScoreCategoryWeightDto>> ReactivateAsync(string categoryName, CancellationToken cancellationToken)
    {
        var normalized = NormalizeName(categoryName);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return OperationResult<ScoreCategoryWeightDto>.Fail("validation", "CategoryName is required.");
        }

        var entity = await _context.ScoreCategoryWeights.FindAsync(new object[] { normalized }, cancellationToken);
        if (entity is null)
        {
            return OperationResult<ScoreCategoryWeightDto>.Fail("not_found", "Record not found.");
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
                return OperationResult<ScoreCategoryWeightDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
            }
        }

        var result = new ScoreCategoryWeightDto(entity.CategoryName, entity.CategoryWeight, entity.Active);
        return OperationResult<ScoreCategoryWeightDto>.Ok(result);
    }

    private static string? NormalizeName(string? value)
        => value?.Trim();

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

public interface IScoreValueWeightService
{
    Task<OperationResult<ScoreValueWeightDto>> GetByIdAsync(string categoryName, string valueName, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<ScoreValueWeightDto>>> GetAllAsync(ScoreValueWeightQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<ScoreValueWeightDto>> CreateAsync(ScoreValueWeightCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<ScoreValueWeightDto>> PatchAsync(string categoryName, string valueName, ScoreValueWeightUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(string categoryName, string valueName, CancellationToken cancellationToken);
    Task<OperationResult<ScoreValueWeightDto>> ReactivateAsync(string categoryName, string valueName, CancellationToken cancellationToken);
}

public sealed class ScoreValueWeightService : IScoreValueWeightService
{
    private const int CategoryNameMaxLength = 60;
    private const int ValueNameMaxLength = 100;
    private readonly ApeironDbContext _context;

    public ScoreValueWeightService(ApeironDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<ScoreValueWeightDto>> GetByIdAsync(string categoryName, string valueName, CancellationToken cancellationToken)
    {
        var normalizedCategory = NormalizeName(categoryName);
        var normalizedValue = NormalizeName(valueName);
        if (string.IsNullOrWhiteSpace(normalizedCategory) || string.IsNullOrWhiteSpace(normalizedValue))
        {
            return OperationResult<ScoreValueWeightDto>.Fail("validation", "CategoryName and ValueName are required.");
        }

        var entity = await _context.ScoreValueWeights.FindAsync(new object[] { normalizedCategory, normalizedValue }, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<ScoreValueWeightDto>.Fail("not_found", "Record not found.");
        }

        var result = new ScoreValueWeightDto(entity.CategoryName, entity.ValueName, entity.ValueWeight, entity.Active);
        return OperationResult<ScoreValueWeightDto>.Ok(result);
    }

    public async Task<OperationResult<PagedResult<ScoreValueWeightDto>>> GetAllAsync(ScoreValueWeightQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);
        var categoryName = NormalizeName(query.CategoryName);
        var valueName = NormalizeName(query.ValueName);

        var dataQuery = _context.ScoreValueWeights.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            dataQuery = dataQuery.Where(e => e.CategoryName == categoryName);
        }

        if (!string.IsNullOrWhiteSpace(valueName))
        {
            dataQuery = dataQuery.Where(e => e.ValueName == valueName);
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
            .OrderBy(e => e.CategoryName)
            .ThenBy(e => e.ValueName)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ScoreValueWeightDto(e.CategoryName, e.ValueName, e.ValueWeight, e.Active))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ScoreValueWeightDto>(items, currentPage, pageSize, totalCount);
        return OperationResult<PagedResult<ScoreValueWeightDto>>.Ok(result);
    }

    public async Task<OperationResult<ScoreValueWeightDto>> CreateAsync(ScoreValueWeightCreateDto dto, CancellationToken cancellationToken)
    {
        var categoryName = NormalizeName(dto.CategoryName);
        var valueName = NormalizeName(dto.ValueName);
        if (string.IsNullOrWhiteSpace(categoryName) || string.IsNullOrWhiteSpace(valueName))
        {
            return OperationResult<ScoreValueWeightDto>.Fail("validation", "CategoryName and ValueName are required.");
        }

        if (categoryName.Length > CategoryNameMaxLength)
        {
            return OperationResult<ScoreValueWeightDto>.Fail("validation", $"CategoryName must be at most {CategoryNameMaxLength} characters.");
        }

        if (valueName.Length > ValueNameMaxLength)
        {
            return OperationResult<ScoreValueWeightDto>.Fail("validation", $"ValueName must be at most {ValueNameMaxLength} characters.");
        }

        if (dto.ValueWeight < 0)
        {
            return OperationResult<ScoreValueWeightDto>.Fail("validation", "ValueWeight must be greater than or equal to 0.");
        }

        var categoryExists = await _context.ScoreCategoryWeights
            .AsNoTracking()
            .AnyAsync(e => e.CategoryName == categoryName && e.Active, cancellationToken);
        if (!categoryExists)
        {
            return OperationResult<ScoreValueWeightDto>.Fail("not_found", "CategoryName not found.");
        }

        var existing = await _context.ScoreValueWeights.FindAsync(new object[] { categoryName, valueName }, cancellationToken);
        if (existing is not null)
        {
            var message = existing.Active
                ? "CategoryName and ValueName already exists."
                : "CategoryName and ValueName already exists as an inactive record.";
            return OperationResult<ScoreValueWeightDto>.Fail("conflict", message);
        }

        var entity = new ScoreValueWeight
        {
            CategoryName = categoryName,
            ValueName = valueName,
            ValueWeight = dto.ValueWeight,
            Active = true
        };

        _context.ScoreValueWeights.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<ScoreValueWeightDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var result = new ScoreValueWeightDto(entity.CategoryName, entity.ValueName, entity.ValueWeight, entity.Active);
        return OperationResult<ScoreValueWeightDto>.Ok(result);
    }

    public async Task<OperationResult<ScoreValueWeightDto>> PatchAsync(string categoryName, string valueName, ScoreValueWeightUpdateDto dto, CancellationToken cancellationToken)
    {
        var normalizedCategory = NormalizeName(categoryName);
        var normalizedValue = NormalizeName(valueName);
        if (string.IsNullOrWhiteSpace(normalizedCategory) || string.IsNullOrWhiteSpace(normalizedValue))
        {
            return OperationResult<ScoreValueWeightDto>.Fail("validation", "CategoryName and ValueName are required.");
        }

        var entity = await _context.ScoreValueWeights.FindAsync(new object[] { normalizedCategory, normalizedValue }, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<ScoreValueWeightDto>.Fail("not_found", "Record not found.");
        }

        if (!dto.ValueWeight.HasValue && !dto.Active.HasValue)
        {
            return OperationResult<ScoreValueWeightDto>.Fail("validation", "At least one field must be provided.");
        }

        if (dto.ValueWeight.HasValue)
        {
            if (dto.ValueWeight.Value < 0)
            {
                return OperationResult<ScoreValueWeightDto>.Fail("validation", "ValueWeight must be greater than or equal to 0.");
            }

            entity.ValueWeight = dto.ValueWeight.Value;
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
            return OperationResult<ScoreValueWeightDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var result = new ScoreValueWeightDto(entity.CategoryName, entity.ValueName, entity.ValueWeight, entity.Active);
        return OperationResult<ScoreValueWeightDto>.Ok(result);
    }

    public async Task<OperationResult<bool>> DeleteAsync(string categoryName, string valueName, CancellationToken cancellationToken)
    {
        var normalizedCategory = NormalizeName(categoryName);
        var normalizedValue = NormalizeName(valueName);
        if (string.IsNullOrWhiteSpace(normalizedCategory) || string.IsNullOrWhiteSpace(normalizedValue))
        {
            return OperationResult<bool>.Fail("validation", "CategoryName and ValueName are required.");
        }

        var entity = await _context.ScoreValueWeights.FindAsync(new object[] { normalizedCategory, normalizedValue }, cancellationToken);
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

    public async Task<OperationResult<ScoreValueWeightDto>> ReactivateAsync(string categoryName, string valueName, CancellationToken cancellationToken)
    {
        var normalizedCategory = NormalizeName(categoryName);
        var normalizedValue = NormalizeName(valueName);
        if (string.IsNullOrWhiteSpace(normalizedCategory) || string.IsNullOrWhiteSpace(normalizedValue))
        {
            return OperationResult<ScoreValueWeightDto>.Fail("validation", "CategoryName and ValueName are required.");
        }

        var entity = await _context.ScoreValueWeights.FindAsync(new object[] { normalizedCategory, normalizedValue }, cancellationToken);
        if (entity is null)
        {
            return OperationResult<ScoreValueWeightDto>.Fail("not_found", "Record not found.");
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
                return OperationResult<ScoreValueWeightDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
            }
        }

        var result = new ScoreValueWeightDto(entity.CategoryName, entity.ValueName, entity.ValueWeight, entity.Active);
        return OperationResult<ScoreValueWeightDto>.Ok(result);
    }

    private static string? NormalizeName(string? value)
        => value?.Trim();

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
