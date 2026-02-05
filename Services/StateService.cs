using Microsoft.EntityFrameworkCore;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;
using Api.Comercial.Repositories;

namespace Api.Comercial.Services;

public interface IStateService
{
    Task<OperationResult<StateDto>> GetByIdAsync(string stateCode, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<StateDto>>> GetAllAsync(StateQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<StateDto>> CreateAsync(StateCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<StateDto>> PatchAsync(string stateCode, StateUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(string stateCode, CancellationToken cancellationToken);
}

public sealed class StateService : IStateService
{
    private readonly IRepository<State, string> _stateRepository;
    private readonly IRepository<Country, string> _countryRepository;

    public StateService(IRepository<State, string> stateRepository, IRepository<Country, string> countryRepository)
    {
        _stateRepository = stateRepository;
        _countryRepository = countryRepository;
    }

    public async Task<OperationResult<StateDto>> GetByIdAsync(string stateCode, CancellationToken cancellationToken)
    {
        var entity = await _stateRepository.GetByIdAsync(stateCode, cancellationToken);
        if (entity is null || !entity.Ativo)
        {
            return OperationResult<StateDto>.Fail("not_found", "Record not found.");
        }

        var result = new StateDto(entity.Code, entity.CountryCode, entity.Name, entity.Ativo);
        return OperationResult<StateDto>.Ok(result);
    }

    public async Task<OperationResult<PagedResult<StateDto>>> GetAllAsync(StateQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var stateCode = query.StateCode?.Trim();
        var countryCode = query.CountryCode?.Trim();
        var stateName = query.StateName?.Trim();

        var dataQuery = _stateRepository.Query(asNoTracking: true).AsQueryable();

        if (!string.IsNullOrWhiteSpace(stateCode))
        {
            dataQuery = dataQuery.Where(e => e.Code == stateCode);
        }

        if (!string.IsNullOrWhiteSpace(countryCode))
        {
            dataQuery = dataQuery.Where(e => e.CountryCode == countryCode);
        }

        if (!string.IsNullOrWhiteSpace(stateName))
        {
            dataQuery = dataQuery.Where(e => e.Name == stateName);
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
            .Select(e => new StateDto(e.Code, e.CountryCode, e.Name, e.Ativo))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<StateDto>(items, currentPage, pageSize, totalCount);
        return OperationResult<PagedResult<StateDto>>.Ok(result);
    }

    public async Task<OperationResult<StateDto>> CreateAsync(StateCreateDto dto, CancellationToken cancellationToken)
    {
        var stateCode = dto.StateCode?.Trim();
        var countryCode = dto.CountryCode?.Trim();
        var stateName = dto.StateName?.Trim();

        if (string.IsNullOrWhiteSpace(stateCode) || string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(stateName))
        {
            return OperationResult<StateDto>.Fail("validation", "StateCode, CountryCode, and StateName are required.");
        }

        var existing = await _stateRepository.GetByIdAsync(stateCode, cancellationToken);
        if (existing is not null)
        {
            var message = existing.Ativo
                ? "StateCode already exists."
                : "StateCode already exists as an inactive record.";
            return OperationResult<StateDto>.Fail("conflict", message);
        }

        var country = await _countryRepository.GetByIdAsync(countryCode, cancellationToken);
        if (country is null)
        {
            return OperationResult<StateDto>.Fail("not_found", "CountryCode not found.");
        }

        var entity = new State
        {
            Code = stateCode,
            CountryCode = countryCode,
            Name = stateName,
            Ativo = true
        };

        await _stateRepository.AddAsync(entity, cancellationToken);

        try
        {
            await _stateRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<StateDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var result = new StateDto(entity.Code, entity.CountryCode, entity.Name, entity.Ativo);
        return OperationResult<StateDto>.Ok(result);
    }

    public async Task<OperationResult<StateDto>> PatchAsync(string stateCode, StateUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _stateRepository.GetByIdAsync(stateCode, cancellationToken);
        if (entity is null || !entity.Ativo)
        {
            return OperationResult<StateDto>.Fail("not_found", "Record not found.");
        }

        var countryCode = dto.CountryCode?.Trim();
        var stateName = dto.StateName?.Trim();

        if (string.IsNullOrWhiteSpace(countryCode) && string.IsNullOrWhiteSpace(stateName) && !dto.Ativo.HasValue)
        {
            return OperationResult<StateDto>.Fail("validation", "At least one field must be provided.");
        }

        if (!string.IsNullOrWhiteSpace(countryCode))
        {
            var country = await _countryRepository.GetByIdAsync(countryCode, cancellationToken);
            if (country is null)
            {
                return OperationResult<StateDto>.Fail("not_found", "CountryCode not found.");
            }

            entity.CountryCode = countryCode;
        }

        if (!string.IsNullOrWhiteSpace(stateName))
        {
            entity.Name = stateName;
        }

        if (dto.Ativo.HasValue)
        {
            entity.Ativo = dto.Ativo.Value;
        }

        try
        {
            await _stateRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<StateDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var result = new StateDto(entity.Code, entity.CountryCode, entity.Name, entity.Ativo);
        return OperationResult<StateDto>.Ok(result);
    }

    public async Task<OperationResult<bool>> DeleteAsync(string stateCode, CancellationToken cancellationToken)
    {
        var entity = await _stateRepository.GetByIdAsync(stateCode, cancellationToken);
        if (entity is null || !entity.Ativo)
        {
            return OperationResult<bool>.Fail("not_found", "Record not found.");
        }

        entity.Ativo = false;

        try
        {
            await _stateRepository.SaveChangesAsync(cancellationToken);
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
