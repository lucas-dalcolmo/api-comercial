using Microsoft.EntityFrameworkCore;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;
using Api.Comercial.Repositories;

namespace Api.Comercial.Services;

public interface ICodeNameService<TEntity>
    where TEntity : CodeNameEntity
{
    Task<OperationResult<CodeNameDto>> GetByIdAsync(string code, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<CodeNameDto>>> GetAllAsync(CodeNameQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<CodeNameDto>> CreateAsync(CodeNameCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<CodeNameDto>> PatchAsync(string code, CodeNameUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(string code, CancellationToken cancellationToken);
    Task<OperationResult<CodeNameDto>> ReactivateAsync(string code, CancellationToken cancellationToken);
}

public sealed class CodeNameService<TEntity> : ICodeNameService<TEntity>
    where TEntity : CodeNameEntity, new()
{
    private readonly IRepository<TEntity, string> _repository;
    private static readonly (int? CodeMax, int? NameMax) _limits = GetLimits();

    public CodeNameService(IRepository<TEntity, string> repository)
    {
        _repository = repository;
    }

    public async Task<OperationResult<CodeNameDto>> GetByIdAsync(string code, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(code, cancellationToken);
        if (entity is null || !entity.Ativo)
        {
            return OperationResult<CodeNameDto>.Fail("not_found", "Record not found.");
        }

        var result = new CodeNameDto(entity.Code, entity.Name, entity.Ativo);
        return OperationResult<CodeNameDto>.Ok(result);
    }

    public async Task<OperationResult<PagedResult<CodeNameDto>>> GetAllAsync(CodeNameQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var code = query.Code?.Trim();
        var name = query.Name?.Trim();
        var dataQuery = _repository.Query(asNoTracking: true).AsQueryable();

        if (!string.IsNullOrWhiteSpace(code))
        {
            dataQuery = dataQuery.Where(e => e.Code == code);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            dataQuery = dataQuery.Where(e => e.Name == name);
        }

        if (query.Ativo.HasValue)
        {
            dataQuery = dataQuery.Where(e => e.Ativo == query.Ativo.Value);
        }
        else
        {
            dataQuery = dataQuery.Where(e => e.Ativo);
        }

        var totalCount = await dataQuery.CountAsync(cancellationToken);
        var items = await dataQuery
            .OrderBy(e => e.Code)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new CodeNameDto(e.Code, e.Name, e.Ativo))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<CodeNameDto>(items, currentPage, pageSize, totalCount);
        return OperationResult<PagedResult<CodeNameDto>>.Ok(result);
    }

    public async Task<OperationResult<CodeNameDto>> CreateAsync(CodeNameCreateDto dto, CancellationToken cancellationToken)
    {
        var code = dto.Code?.Trim();
        var name = dto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<CodeNameDto>.Fail("validation", "Code and Name are required.");
        }

        var lengthError = ValidateLengths(code, name);
        if (lengthError is not null)
        {
            return lengthError;
        }

        var exists = await _repository.GetByIdAsync(code, cancellationToken);
        if (exists is not null)
        {
            var message = exists.Ativo
                ? "Code already exists."
                : "Code already exists as an inactive record.";
            return OperationResult<CodeNameDto>.Fail("conflict", message);
        }

        var entity = new TEntity
        {
            Code = code,
            Name = name,
            Ativo = true
        };

        await _repository.AddAsync(entity, cancellationToken);

        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<CodeNameDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var result = new CodeNameDto(entity.Code, entity.Name, entity.Ativo);
        return OperationResult<CodeNameDto>.Ok(result);
    }

    public async Task<OperationResult<CodeNameDto>> PatchAsync(string code, CodeNameUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(code, cancellationToken);
        if (entity is null || !entity.Ativo)
        {
            return OperationResult<CodeNameDto>.Fail("not_found", "Record not found.");
        }

        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) && !dto.Ativo.HasValue)
        {
            return OperationResult<CodeNameDto>.Fail("validation", "At least one field must be provided.");
        }

        var lengthError = ValidateLengths(code, name);
        if (lengthError is not null)
        {
            return lengthError;
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            entity.Name = name;
        }

        if (dto.Ativo.HasValue)
        {
            entity.Ativo = dto.Ativo.Value;
        }

        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<CodeNameDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var result = new CodeNameDto(entity.Code, entity.Name, entity.Ativo);
        return OperationResult<CodeNameDto>.Ok(result);
    }

    public async Task<OperationResult<bool>> DeleteAsync(string code, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(code, cancellationToken);
        if (entity is null || !entity.Ativo)
        {
            return OperationResult<bool>.Fail("not_found", "Record not found.");
        }

        entity.Ativo = false;

        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<bool>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<bool>.Ok(true);
    }

    public async Task<OperationResult<CodeNameDto>> ReactivateAsync(string code, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(code, cancellationToken);
        if (entity is null)
        {
            return OperationResult<CodeNameDto>.Fail("not_found", "Record not found.");
        }

        if (!entity.Ativo)
        {
            entity.Ativo = true;
            try
            {
                await _repository.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                return OperationResult<CodeNameDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
            }
        }

        var result = new CodeNameDto(entity.Code, entity.Name, entity.Ativo);
        return OperationResult<CodeNameDto>.Ok(result);
    }

    private static (int? CodeMax, int? NameMax) GetLimits()
    {
        var type = typeof(TEntity);
        if (type == typeof(Country))
        {
            return (2, 80);
        }

        if (type == typeof(Currency))
        {
            return (3, 50);
        }

        return (null, null);
    }

    private static OperationResult<CodeNameDto>? ValidateLengths(string? code, string? name)
    {
        if (_limits.CodeMax.HasValue && !string.IsNullOrWhiteSpace(code) && code.Length > _limits.CodeMax.Value)
        {
            return OperationResult<CodeNameDto>.Fail("validation", $"Code must be at most {_limits.CodeMax.Value} characters.");
        }

        if (_limits.NameMax.HasValue && !string.IsNullOrWhiteSpace(name) && name.Length > _limits.NameMax.Value)
        {
            return OperationResult<CodeNameDto>.Fail("validation", $"Name must be at most {_limits.NameMax.Value} characters.");
        }

        return null;
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
