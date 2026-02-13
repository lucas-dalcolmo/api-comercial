using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;

namespace Api.Comercial.Services;

public interface IEmployeeContractBenefitService
{
    Task<OperationResult<EmployeeContractBenefitDto>> GetByIdAsync(int contractId, int benefitId, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<EmployeeContractBenefitDto>>> GetAllAsync(EmployeeContractBenefitQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeContractBenefitDto>> CreateAsync(int contractId, EmployeeContractBenefitCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeContractBenefitDto>> PatchAsync(int contractId, int benefitId, EmployeeContractBenefitUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(int contractId, int benefitId, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeContractBenefitDto>> ReactivateAsync(int contractId, int benefitId, CancellationToken cancellationToken);
}

public sealed class EmployeeContractBenefitService : IEmployeeContractBenefitService
{
    private readonly ApeironDbContext _context;
    private readonly IBenefitFormulaEvaluator _formulaEvaluator;
    private readonly IBenefitFormulaVariableResolver _variableResolver;

    public EmployeeContractBenefitService(
        ApeironDbContext context,
        IBenefitFormulaEvaluator formulaEvaluator,
        IBenefitFormulaVariableResolver variableResolver)
    {
        _context = context;
        _formulaEvaluator = formulaEvaluator;
        _variableResolver = variableResolver;
    }

    public async Task<OperationResult<EmployeeContractBenefitDto>> GetByIdAsync(int contractId, int benefitId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContractBenefits
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ContractId == contractId && b.Id == benefitId, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("not_found", "Record not found.");
        }

        return await MapBenefitAsync(entity, domainMode: true, cancellationToken);
    }

    public async Task<OperationResult<PagedResult<EmployeeContractBenefitDto>>> GetAllAsync(EmployeeContractBenefitQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dataQuery = _context.EmployeeContractBenefits.AsNoTracking().AsQueryable();

        if (query.ContractId.HasValue)
        {
            dataQuery = dataQuery.Where(e => e.ContractId == query.ContractId.Value);
        }

        if (query.BenefitTypeId.HasValue)
        {
            dataQuery = dataQuery.Where(e => e.BenefitTypeId == query.BenefitTypeId.Value);
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
        var entities = await dataQuery
            .OrderByDescending(e => e.Id)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<EmployeeContractBenefitDto>(entities.Count);
        foreach (var entity in entities)
        {
            var mapped = await MapBenefitAsync(entity, domainMode: entity.Active, cancellationToken);
            if (!mapped.Success)
            {
                return OperationResult<PagedResult<EmployeeContractBenefitDto>>.Fail(mapped.ErrorCode!, mapped.ErrorMessage!);
            }

            items.Add(mapped.Data!);
        }

        var result = new PagedResult<EmployeeContractBenefitDto>(items, currentPage, pageSize, totalCount);
        return OperationResult<PagedResult<EmployeeContractBenefitDto>>.Ok(result);
    }

    public async Task<OperationResult<EmployeeContractBenefitDto>> CreateAsync(int contractId, EmployeeContractBenefitCreateDto dto, CancellationToken cancellationToken)
    {
        var contract = await _context.EmployeeContracts.FirstOrDefaultAsync(e => e.Id == contractId, cancellationToken);
        if (contract is null || !contract.Active)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("not_found", "Contract not found.");
        }

        if (dto.BenefitTypeId is null)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("validation", "BenefitTypeId is required.");
        }

        var hasDuplicate = await HasDuplicateBenefitTypeAsync(contractId, dto.BenefitTypeId.Value, null, cancellationToken);
        if (hasDuplicate)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("conflict", "BenefitTypeId already exists for this contract.");
        }

        var normalizedFormula = dto.Formula?.Trim();
        var validation = ValidateModel(dto.IsFormula, dto.Value, normalizedFormula);
        if (!validation.Success)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("validation", validation.ErrorMessage!);
        }

        var evaluated = await EvaluateCalculatedValueAsync(contractId, dto.IsFormula, dto.Value, normalizedFormula, domainMode: false, cancellationToken);
        if (!evaluated.Success)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail(evaluated.ErrorCode!, evaluated.ErrorMessage!);
        }

        var entity = new EmployeeContractBenefit
        {
            ContractId = contractId,
            BenefitTypeId = dto.BenefitTypeId,
            Value = dto.Value,
            IsFormula = dto.IsFormula,
            Formula = dto.IsFormula ? normalizedFormula : null,
            Active = true
        };

        _context.EmployeeContractBenefits.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<EmployeeContractBenefitDto>.Ok(new EmployeeContractBenefitDto(
            entity.Id,
            entity.ContractId,
            entity.BenefitTypeId,
            entity.Value,
            entity.IsFormula,
            entity.Formula,
            evaluated.Data,
            entity.Active));
    }

    public async Task<OperationResult<EmployeeContractBenefitDto>> PatchAsync(int contractId, int benefitId, EmployeeContractBenefitUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContractBenefits
            .FirstOrDefaultAsync(b => b.ContractId == contractId && b.Id == benefitId, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("not_found", "Record not found.");
        }

        if (dto.BenefitTypeId is null
            && dto.Value is null
            && dto.IsFormula is null
            && dto.Formula is null
            && !dto.Active.HasValue)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("validation", "At least one field must be provided.");
        }

        var newBenefitTypeId = dto.BenefitTypeId ?? entity.BenefitTypeId;
        if (newBenefitTypeId is null)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("validation", "BenefitTypeId is required.");
        }

        if (dto.BenefitTypeId.HasValue)
        {
            var hasDuplicate = await HasDuplicateBenefitTypeAsync(contractId, dto.BenefitTypeId.Value, entity.Id, cancellationToken);
            if (hasDuplicate)
            {
                return OperationResult<EmployeeContractBenefitDto>.Fail("conflict", "BenefitTypeId already exists for this contract.");
            }
        }

        var newIsFormula = dto.IsFormula ?? entity.IsFormula;
        var newFormula = dto.Formula is not null ? dto.Formula.Trim() : entity.Formula;
        var newValue = dto.Value ?? entity.Value;

        var validation = ValidateModel(newIsFormula, newValue, newFormula);
        if (!validation.Success)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("validation", validation.ErrorMessage!);
        }

        var evaluated = await EvaluateCalculatedValueAsync(contractId, newIsFormula, newValue, newFormula, domainMode: false, cancellationToken);
        if (!evaluated.Success)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail(evaluated.ErrorCode!, evaluated.ErrorMessage!);
        }

        entity.BenefitTypeId = newBenefitTypeId.Value;
        entity.IsFormula = newIsFormula;
        entity.Formula = newIsFormula ? newFormula : null;
        entity.Value = newValue;
        if (dto.Active.HasValue) entity.Active = dto.Active.Value;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<EmployeeContractBenefitDto>.Ok(new EmployeeContractBenefitDto(
            entity.Id,
            entity.ContractId,
            entity.BenefitTypeId,
            entity.Value,
            entity.IsFormula,
            entity.Formula,
            evaluated.Data,
            entity.Active));
    }

    public async Task<OperationResult<bool>> DeleteAsync(int contractId, int benefitId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContractBenefits
            .FirstOrDefaultAsync(b => b.ContractId == contractId && b.Id == benefitId, cancellationToken);

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

    public async Task<OperationResult<EmployeeContractBenefitDto>> ReactivateAsync(int contractId, int benefitId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContractBenefits
            .FirstOrDefaultAsync(b => b.ContractId == contractId && b.Id == benefitId, cancellationToken);

        if (entity is null)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("not_found", "Record not found.");
        }

        if (entity.BenefitTypeId is null)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("validation", "BenefitTypeId is required.");
        }

        var hasDuplicate = await HasDuplicateBenefitTypeAsync(contractId, entity.BenefitTypeId.Value, entity.Id, cancellationToken);
        if (hasDuplicate)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("conflict", "BenefitTypeId already exists for this contract.");
        }

        var validation = ValidateModel(entity.IsFormula, entity.Value, entity.Formula);
        if (!validation.Success)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail("validation", validation.ErrorMessage!);
        }

        var evaluated = await EvaluateCalculatedValueAsync(contractId, entity.IsFormula, entity.Value, entity.Formula, domainMode: false, cancellationToken);
        if (!evaluated.Success)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail(evaluated.ErrorCode!, evaluated.ErrorMessage!);
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
                return OperationResult<EmployeeContractBenefitDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
            }
        }

        return OperationResult<EmployeeContractBenefitDto>.Ok(new EmployeeContractBenefitDto(
            entity.Id,
            entity.ContractId,
            entity.BenefitTypeId,
            entity.Value,
            entity.IsFormula,
            entity.Formula,
            evaluated.Data,
            entity.Active));
    }

    private async Task<bool> HasDuplicateBenefitTypeAsync(int contractId, int benefitTypeId, int? exceptBenefitId, CancellationToken cancellationToken)
    {
        return await _context.EmployeeContractBenefits.AnyAsync(
            b => b.ContractId == contractId
                 && b.BenefitTypeId == benefitTypeId
                 && b.Active
                 && (!exceptBenefitId.HasValue || b.Id != exceptBenefitId.Value),
            cancellationToken);
    }

    private async Task<OperationResult<decimal>> EvaluateCalculatedValueAsync(
        int contractId,
        bool isFormula,
        decimal? value,
        string? formula,
        bool domainMode,
        CancellationToken cancellationToken)
    {
        if (!isFormula)
        {
            if (!value.HasValue)
            {
                var code = domainMode ? "domain_error" : "validation";
                return OperationResult<decimal>.Fail(code, "Unable to calculate fixed benefit because Value is null.");
            }

            return OperationResult<decimal>.Ok(value.Value);
        }

        if (string.IsNullOrWhiteSpace(formula))
        {
            var code = domainMode ? "domain_error" : "validation";
            return OperationResult<decimal>.Fail(code, "Formula is required when IsFormula is true.");
        }

        var variables = await _variableResolver.ResolveAsync(contractId, formula, cancellationToken);
        if (!variables.Success)
        {
            var code = domainMode ? "domain_error" : "validation";
            return OperationResult<decimal>.Fail(code, variables.ErrorMessage!);
        }

        if (!_formulaEvaluator.TryEvaluate(formula, variables.Data!, out var calculated, out var formulaError))
        {
            var code = domainMode ? "domain_error" : "validation";
            return OperationResult<decimal>.Fail(code, $"Invalid formula: {formulaError}");
        }

        return OperationResult<decimal>.Ok(calculated);
    }

    private static OperationResult<bool> ValidateModel(bool isFormula, decimal? value, string? formula)
    {
        if (!isFormula)
        {
            if (!value.HasValue)
            {
                return OperationResult<bool>.Fail("validation", "Value is required when IsFormula is false.");
            }

            return OperationResult<bool>.Ok(true);
        }

        if (string.IsNullOrWhiteSpace(formula))
        {
            return OperationResult<bool>.Fail("validation", "Formula is required when IsFormula is true.");
        }

        return OperationResult<bool>.Ok(true);
    }

    private async Task<OperationResult<EmployeeContractBenefitDto>> MapBenefitAsync(
        EmployeeContractBenefit entity,
        bool domainMode,
        CancellationToken cancellationToken)
    {
        var evaluated = await EvaluateCalculatedValueAsync(
            entity.ContractId,
            entity.IsFormula,
            entity.Value,
            entity.Formula,
            domainMode,
            cancellationToken);

        if (!evaluated.Success)
        {
            return OperationResult<EmployeeContractBenefitDto>.Fail(evaluated.ErrorCode!, evaluated.ErrorMessage!);
        }

        return OperationResult<EmployeeContractBenefitDto>.Ok(new EmployeeContractBenefitDto(
            entity.Id,
            entity.ContractId,
            entity.BenefitTypeId,
            entity.Value,
            entity.IsFormula,
            entity.Formula,
            evaluated.Data,
            entity.Active));
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
