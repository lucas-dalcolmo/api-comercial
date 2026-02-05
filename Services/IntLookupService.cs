using Microsoft.EntityFrameworkCore;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;
using Api.Comercial.Repositories;

namespace Api.Comercial.Services;

public interface IIntLookupService<TEntity>
    where TEntity : IntLookupEntity
{
    Task<OperationResult<LookupDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<LookupDto>>> GetAllAsync(LookupQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<LookupDto>> CreateAsync(LookupCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<LookupDto>> PatchAsync(int id, LookupUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed class IntLookupService<TEntity> : IIntLookupService<TEntity>
    where TEntity : IntLookupEntity, new()
{
    private readonly IRepository<TEntity, int> _repository;

    public IntLookupService(IRepository<TEntity, int> repository)
    {
        _repository = repository;
    }

    public async Task<OperationResult<LookupDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return OperationResult<LookupDto>.Fail("not_found", "Record not found.");
        }

        var result = new LookupDto(entity.Id, entity.Name);
        return OperationResult<LookupDto>.Ok(result);
    }

    public async Task<OperationResult<PagedResult<LookupDto>>> GetAllAsync(LookupQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var name = query.Name?.Trim();
        var dataQuery = _repository.Query(asNoTracking: true).AsQueryable();

        if (query.Id.HasValue)
        {
            dataQuery = dataQuery.Where(e => e.Id == query.Id.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            dataQuery = dataQuery.Where(e => e.Name == name);
        }

        var totalCount = await dataQuery.CountAsync(cancellationToken);
        var items = await dataQuery
            .OrderBy(e => e.Id)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new LookupDto(e.Id, e.Name))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<LookupDto>(items, currentPage, pageSize, totalCount);
        return OperationResult<PagedResult<LookupDto>>.Ok(result);
    }

    public async Task<OperationResult<LookupDto>> CreateAsync(LookupCreateDto dto, CancellationToken cancellationToken)
    {
        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<LookupDto>.Fail("validation", "Name is required.");
        }

        var exists = await _repository
            .Query(asNoTracking: true)
            .AnyAsync(e => e.Name == name, cancellationToken);

        if (exists)
        {
            return OperationResult<LookupDto>.Fail("conflict", "Name already exists.");
        }

        var entity = new TEntity
        {
            Name = name
        };

        await _repository.AddAsync(entity, cancellationToken);

        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<LookupDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var result = new LookupDto(entity.Id, entity.Name);
        return OperationResult<LookupDto>.Ok(result);
    }

    public async Task<OperationResult<LookupDto>> PatchAsync(int id, LookupUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return OperationResult<LookupDto>.Fail("not_found", "Record not found.");
        }

        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<LookupDto>.Fail("validation", "Name is required.");
        }

        var exists = await _repository
            .Query(asNoTracking: true)
            .AnyAsync(e => e.Name == name && e.Id != id, cancellationToken);

        if (exists)
        {
            return OperationResult<LookupDto>.Fail("conflict", "Name already exists.");
        }

        entity.Name = name;

        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<LookupDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var result = new LookupDto(entity.Id, entity.Name);
        return OperationResult<LookupDto>.Ok(result);
    }

    public async Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
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
