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
}

public sealed class CodeNameService<TEntity> : ICodeNameService<TEntity>
    where TEntity : CodeNameEntity, new()
{
    private readonly IRepository<TEntity, string> _repository;

    public CodeNameService(IRepository<TEntity, string> repository)
    {
        _repository = repository;
    }

    public async Task<OperationResult<CodeNameDto>> GetByIdAsync(string code, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(code, cancellationToken);
        if (entity is null)
        {
            return OperationResult<CodeNameDto>.Fail("not_found", "Record not found.");
        }

        var result = new CodeNameDto(entity.Code, entity.Name);
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

        var totalCount = await dataQuery.CountAsync(cancellationToken);
        var items = await dataQuery
            .OrderBy(e => e.Code)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new CodeNameDto(e.Code, e.Name))
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

        var exists = await _repository.GetByIdAsync(code, cancellationToken);
        if (exists is not null)
        {
            return OperationResult<CodeNameDto>.Fail("conflict", "Code already exists.");
        }

        var entity = new TEntity
        {
            Code = code,
            Name = name
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

        var result = new CodeNameDto(entity.Code, entity.Name);
        return OperationResult<CodeNameDto>.Ok(result);
    }

    public async Task<OperationResult<CodeNameDto>> PatchAsync(string code, CodeNameUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(code, cancellationToken);
        if (entity is null)
        {
            return OperationResult<CodeNameDto>.Fail("not_found", "Record not found.");
        }

        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<CodeNameDto>.Fail("validation", "Name is required.");
        }

        entity.Name = name;

        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<CodeNameDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var result = new CodeNameDto(entity.Code, entity.Name);
        return OperationResult<CodeNameDto>.Ok(result);
    }

    public async Task<OperationResult<bool>> DeleteAsync(string code, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(code, cancellationToken);
        if (entity is null)
        {
            return OperationResult<bool>.Fail("not_found", "Record not found.");
        }

        _repository.Remove(entity);

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
