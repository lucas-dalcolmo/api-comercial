using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;

namespace Api.Comercial.Services;

public interface IProposalService
{
    Task<OperationResult<ProposalDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<ProposalDto>>> GetAllAsync(ProposalQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<ProposalDto>> CreateAsync(ProposalCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<ProposalDto>> PatchAsync(int id, ProposalUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<IReadOnlyList<ProposalEmployeeDto>>> GetEmployeesAsync(int proposalId, CancellationToken cancellationToken);
    Task<OperationResult<ProposalEmployeeDto>> AddEmployeeAsync(int proposalId, ProposalEmployeeAddDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> RemoveEmployeeAsync(int proposalId, int proposalEmployeeId, CancellationToken cancellationToken);
}

public sealed class ProposalService : IProposalService
{
    private const int TitleMaxLength = 200;
    private const int StatusMaxLength = 40;
    private const decimal HourlyDivisor = 220m;

    private readonly ApeironDbContext _context;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IBenefitFormulaVariableResolver _formulaResolver;
    private readonly IBenefitFormulaEvaluator _formulaEvaluator;

    public ProposalService(
        ApeironDbContext context,
        IHtmlSanitizerService htmlSanitizer,
        IBenefitFormulaVariableResolver formulaResolver,
        IBenefitFormulaEvaluator formulaEvaluator)
    {
        _context = context;
        _htmlSanitizer = htmlSanitizer;
        _formulaResolver = formulaResolver;
        _formulaEvaluator = formulaEvaluator;
    }

    public async Task<OperationResult<ProposalDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.Proposals
            .AsNoTracking()
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<ProposalDto>.Fail("not_found", "Record not found.");
        }

        return OperationResult<ProposalDto>.Ok(Map(entity));
    }

    public async Task<OperationResult<PagedResult<ProposalDto>>> GetAllAsync(ProposalQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);
        var normalizedStatus = NormalizeStatus(query.Status);

        var dataQuery = _context.Proposals
            .AsNoTracking()
            .Include(p => p.Client)
            .AsQueryable();

        if (query.ClientId.HasValue)
        {
            dataQuery = dataQuery.Where(p => p.ClientId == query.ClientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            dataQuery = dataQuery.Where(p => p.Status == normalizedStatus);
        }

        if (query.Active.HasValue)
        {
            dataQuery = dataQuery.Where(p => p.Active == query.Active.Value);
        }
        else
        {
            dataQuery = dataQuery.Where(p => p.Active);
        }

        var totalCount = await dataQuery.CountAsync(cancellationToken);
        var items = await dataQuery
            .OrderByDescending(p => p.UpdatedAt)
            .ThenByDescending(p => p.Id)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProposalDto(
                p.Id,
                p.ClientId,
                p.Client != null ? p.Client.Name : string.Empty,
                p.OpportunityId,
                p.Title,
                p.ObjectiveHtml,
                p.ProjectHours,
                p.GlobalMarginPercent,
                p.Status,
                p.TotalCost,
                p.TotalSellPrice,
                p.Active,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return OperationResult<PagedResult<ProposalDto>>.Ok(new PagedResult<ProposalDto>(items, currentPage, pageSize, totalCount));
    }

    public async Task<OperationResult<ProposalDto>> CreateAsync(ProposalCreateDto dto, CancellationToken cancellationToken)
    {
        var title = dto.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return OperationResult<ProposalDto>.Fail("validation", "Title is required.");
        }

        if (title.Length > TitleMaxLength)
        {
            return OperationResult<ProposalDto>.Fail("validation", $"Title must be at most {TitleMaxLength} characters.");
        }

        if (dto.GlobalMarginPercent < 0)
        {
            return OperationResult<ProposalDto>.Fail("validation", "GlobalMarginPercent must be greater than or equal to 0.");
        }

        if (dto.ProjectHours <= 0)
        {
            return OperationResult<ProposalDto>.Fail("validation", "ProjectHours must be greater than 0.");
        }

        if (!dto.OpportunityId.HasValue || dto.OpportunityId.Value <= 0)
        {
            return OperationResult<ProposalDto>.Fail("validation", "OpportunityId is required.");
        }

        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == dto.ClientId && c.Active, cancellationToken);
        if (client is null)
        {
            return OperationResult<ProposalDto>.Fail("not_found", "Client not found.");
        }

        var opportunity = await _context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == dto.OpportunityId.Value && o.Active, cancellationToken);
        if (opportunity is null)
        {
            return OperationResult<ProposalDto>.Fail("not_found", "Opportunity not found.");
        }

        if (opportunity.ClientId != dto.ClientId)
        {
            return OperationResult<ProposalDto>.Fail("domain_error", "Opportunity does not belong to the selected client.");
        }

        var duplicateOpportunity = await _context.Proposals
            .AsNoTracking()
            .AnyAsync(p => p.OpportunityId == dto.OpportunityId.Value && p.Active, cancellationToken);
        if (duplicateOpportunity)
        {
            return OperationResult<ProposalDto>.Fail("conflict", "This opportunity already has an active proposal.");
        }

        var status = NormalizeStatus(dto.Status);
        if (string.IsNullOrWhiteSpace(status))
        {
            status = ProposalStatus.Draft;
        }

        if (!ProposalStatus.Allowed.Contains(status))
        {
            return OperationResult<ProposalDto>.Fail("validation", "Invalid status.");
        }

        var now = DateTime.UtcNow;
        var entity = new Proposal
        {
            ClientId = dto.ClientId,
            OpportunityId = dto.OpportunityId,
            Title = title,
            ObjectiveHtml = _htmlSanitizer.Sanitize(dto.ObjectiveHtml),
            ProjectHours = decimal.Round(dto.ProjectHours, 2, MidpointRounding.AwayFromZero),
            GlobalMarginPercent = decimal.Round(dto.GlobalMarginPercent, 4, MidpointRounding.AwayFromZero),
            Status = status,
            TotalCost = 0m,
            TotalSellPrice = 0m,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Proposals.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<ProposalDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        entity.Client = client;
        return OperationResult<ProposalDto>.Ok(Map(entity));
    }

    public async Task<OperationResult<ProposalDto>> PatchAsync(int id, ProposalUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.Proposals
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<ProposalDto>.Fail("not_found", "Record not found.");
        }

        if (dto.ClientId is null
            && dto.OpportunityId is null
            && dto.Title is null
            && dto.ObjectiveHtml is null
            && dto.ProjectHours is null
            && dto.GlobalMarginPercent is null
            && dto.Status is null
            && !dto.Active.HasValue)
        {
            return OperationResult<ProposalDto>.Fail("validation", "At least one field must be provided.");
        }

        if (dto.ClientId.HasValue)
        {
            if (entity.OpportunityId.HasValue && entity.ClientId != dto.ClientId.Value)
            {
                return OperationResult<ProposalDto>.Fail("domain_error", "Cannot change client for an opportunity-linked proposal.");
            }

            var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == dto.ClientId.Value && c.Active, cancellationToken);
            if (client is null)
            {
                return OperationResult<ProposalDto>.Fail("not_found", "Client not found.");
            }

            entity.ClientId = client.Id;
        }

        if (dto.OpportunityId.HasValue)
        {
            if (dto.OpportunityId.Value <= 0)
            {
                return OperationResult<ProposalDto>.Fail("validation", "OpportunityId must be greater than 0.");
            }

            if (entity.OpportunityId.HasValue && entity.OpportunityId.Value != dto.OpportunityId.Value)
            {
                return OperationResult<ProposalDto>.Fail("domain_error", "OpportunityId cannot be changed after proposal creation.");
            }

            var opportunity = await _context.Opportunities
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == dto.OpportunityId.Value && o.Active, cancellationToken);
            if (opportunity is null)
            {
                return OperationResult<ProposalDto>.Fail("not_found", "Opportunity not found.");
            }

            var targetClientId = dto.ClientId ?? entity.ClientId;
            if (opportunity.ClientId != targetClientId)
            {
                return OperationResult<ProposalDto>.Fail("domain_error", "Opportunity does not belong to the selected client.");
            }

            if (!entity.OpportunityId.HasValue)
            {
                var duplicateOpportunity = await _context.Proposals
                    .AsNoTracking()
                    .AnyAsync(p => p.Id != entity.Id && p.OpportunityId == dto.OpportunityId.Value && p.Active, cancellationToken);
                if (duplicateOpportunity)
                {
                    return OperationResult<ProposalDto>.Fail("conflict", "This opportunity already has an active proposal.");
                }

                entity.OpportunityId = dto.OpportunityId.Value;
            }
        }

        if (dto.Title is not null)
        {
            var title = dto.Title.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return OperationResult<ProposalDto>.Fail("validation", "Title is required.");
            }

            if (title.Length > TitleMaxLength)
            {
                return OperationResult<ProposalDto>.Fail("validation", $"Title must be at most {TitleMaxLength} characters.");
            }

            entity.Title = title;
        }

        if (dto.ObjectiveHtml is not null)
        {
            entity.ObjectiveHtml = _htmlSanitizer.Sanitize(dto.ObjectiveHtml);
        }

        if (dto.ProjectHours.HasValue)
        {
            if (ProposalStatus.IsClosed(entity.Status))
            {
                return OperationResult<ProposalDto>.Fail("domain_error", "Cannot change project hours for a closed proposal.");
            }

            if (dto.ProjectHours.Value <= 0)
            {
                return OperationResult<ProposalDto>.Fail("validation", "ProjectHours must be greater than 0.");
            }

            entity.ProjectHours = decimal.Round(dto.ProjectHours.Value, 2, MidpointRounding.AwayFromZero);

            var activeItems = await _context.ProposalEmployees
                .Where(pe => pe.ProposalId == entity.Id && pe.Active)
                .ToListAsync(cancellationToken);

            foreach (var item in activeItems)
            {
                var sell = RoundCurrency(item.CostSnapshot * (1m + (entity.GlobalMarginPercent / 100m)));
                item.SellPriceSnapshot = sell;
                item.HourlyValueSnapshot = RoundHourly(sell / HourlyDivisor);
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        if (dto.GlobalMarginPercent.HasValue)
        {
            if (ProposalStatus.IsClosed(entity.Status))
            {
                return OperationResult<ProposalDto>.Fail("domain_error", "Cannot change margin for a closed proposal.");
            }

            if (dto.GlobalMarginPercent.Value < 0)
            {
                return OperationResult<ProposalDto>.Fail("validation", "GlobalMarginPercent must be greater than or equal to 0.");
            }

            var margin = decimal.Round(dto.GlobalMarginPercent.Value, 4, MidpointRounding.AwayFromZero);
            entity.GlobalMarginPercent = margin;

            var activeItems = await _context.ProposalEmployees
                .Where(pe => pe.ProposalId == entity.Id && pe.Active)
                .ToListAsync(cancellationToken);

            foreach (var item in activeItems)
            {
                item.MarginPercentApplied = margin;
                var sell = RoundCurrency(item.CostSnapshot * (1m + (margin / 100m)));
                item.SellPriceSnapshot = sell;
                item.HourlyValueSnapshot = RoundHourly(sell / HourlyDivisor);
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        if (dto.Status is not null)
        {
            var status = NormalizeStatus(dto.Status);
            if (string.IsNullOrWhiteSpace(status) || status.Length > StatusMaxLength)
            {
                return OperationResult<ProposalDto>.Fail("validation", "Invalid status.");
            }

            if (!ProposalStatus.Allowed.Contains(status))
            {
                return OperationResult<ProposalDto>.Fail("validation", "Invalid status.");
            }

            entity.Status = status;
        }

        if (dto.Active.HasValue)
        {
            entity.Active = dto.Active.Value;
        }

        await RecalculateProposalTotalsAsync(entity.Id, cancellationToken);
        entity.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<ProposalDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        await _context.Entry(entity).Reference(p => p.Client).LoadAsync(cancellationToken);
        return OperationResult<ProposalDto>.Ok(Map(entity));
    }

    public async Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.Proposals.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<bool>.Fail("not_found", "Record not found.");
        }

        entity.Active = false;
        entity.UpdatedAt = DateTime.UtcNow;

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

    public async Task<OperationResult<IReadOnlyList<ProposalEmployeeDto>>> GetEmployeesAsync(int proposalId, CancellationToken cancellationToken)
    {
        var proposalExists = await _context.Proposals.AnyAsync(p => p.Id == proposalId && p.Active, cancellationToken);
        if (!proposalExists)
        {
            return OperationResult<IReadOnlyList<ProposalEmployeeDto>>.Fail("not_found", "Proposal not found.");
        }

        var items = await _context.ProposalEmployees
            .AsNoTracking()
            .Where(pe => pe.ProposalId == proposalId && pe.Active)
            .Join(
                _context.Employees.AsNoTracking(),
                pe => pe.EmployeeId,
                e => e.Id,
                (pe, e) => new
                {
                    ProposalEmployee = pe,
                    EmployeeName = e.FullName
                })
            .OrderBy(x => x.EmployeeName)
            .Select(x => new ProposalEmployeeDto(
                x.ProposalEmployee.Id,
                x.ProposalEmployee.Id,
                x.ProposalEmployee.ProposalId,
                x.ProposalEmployee.EmployeeId,
                x.EmployeeName,
                x.ProposalEmployee.CostSnapshot,
                x.ProposalEmployee.MarginPercentApplied,
                x.ProposalEmployee.SellPriceSnapshot,
                x.ProposalEmployee.HourlyValueSnapshot,
                x.ProposalEmployee.Active,
                x.ProposalEmployee.CreatedAt,
                x.ProposalEmployee.UpdatedAt))
            .ToListAsync(cancellationToken);

        return OperationResult<IReadOnlyList<ProposalEmployeeDto>>.Ok(items);
    }

    public async Task<OperationResult<ProposalEmployeeDto>> AddEmployeeAsync(int proposalId, ProposalEmployeeAddDto dto, CancellationToken cancellationToken)
    {
        var proposal = await _context.Proposals.FirstOrDefaultAsync(p => p.Id == proposalId && p.Active, cancellationToken);
        if (proposal is null)
        {
            return OperationResult<ProposalEmployeeDto>.Fail("not_found", "Proposal not found.");
        }

        if (ProposalStatus.IsClosed(proposal.Status))
        {
            return OperationResult<ProposalEmployeeDto>.Fail("domain_error", "Cannot change employees for a closed proposal.");
        }

        if (dto.EmployeeId <= 0)
        {
            return OperationResult<ProposalEmployeeDto>.Fail("validation", "EmployeeId is required.");
        }

        var duplicateActive = await _context.ProposalEmployees
            .AnyAsync(pe => pe.ProposalId == proposalId && pe.EmployeeId == dto.EmployeeId && pe.Active, cancellationToken);
        if (duplicateActive)
        {
            return OperationResult<ProposalEmployeeDto>.Fail("conflict", "Employee already exists in this proposal.");
        }

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && e.Active, cancellationToken);
        if (employee is null)
        {
            return OperationResult<ProposalEmployeeDto>.Fail("not_found", "Employee not found.");
        }

        var costResult = await ResolveEmployeeCostSnapshotAsync(employee.Id, cancellationToken);
        if (!costResult.Success)
        {
            return OperationResult<ProposalEmployeeDto>.Fail(costResult.ErrorCode!, costResult.ErrorMessage!);
        }

        var margin = proposal.GlobalMarginPercent;
        var costSnapshot = costResult.Data;
        var sellSnapshot = RoundCurrency(costSnapshot * (1m + (margin / 100m)));
        var hourlySnapshot = RoundHourly(sellSnapshot / HourlyDivisor);
        var now = DateTime.UtcNow;

        var item = new ProposalEmployee
        {
            ProposalId = proposalId,
            EmployeeId = employee.Id,
            CostSnapshot = costSnapshot,
            MarginPercentApplied = margin,
            SellPriceSnapshot = sellSnapshot,
            HourlyValueSnapshot = hourlySnapshot,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.ProposalEmployees.Add(item);

        await RecalculateProposalTotalsAsync(proposalId, cancellationToken);
        proposal.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<ProposalEmployeeDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var dtoResult = new ProposalEmployeeDto(
            item.Id,
            item.Id,
            item.ProposalId,
            item.EmployeeId,
            employee.FullName,
            item.CostSnapshot,
            item.MarginPercentApplied,
            item.SellPriceSnapshot,
            item.HourlyValueSnapshot,
            item.Active,
            item.CreatedAt,
            item.UpdatedAt);

        return OperationResult<ProposalEmployeeDto>.Ok(dtoResult);
    }

    public async Task<OperationResult<bool>> RemoveEmployeeAsync(int proposalId, int proposalEmployeeId, CancellationToken cancellationToken)
    {
        var proposal = await _context.Proposals.FirstOrDefaultAsync(p => p.Id == proposalId && p.Active, cancellationToken);
        if (proposal is null)
        {
            return OperationResult<bool>.Fail("not_found", "Proposal not found.");
        }

        if (ProposalStatus.IsClosed(proposal.Status))
        {
            return OperationResult<bool>.Fail("domain_error", "Cannot change employees for a closed proposal.");
        }

        var item = await _context.ProposalEmployees
            .FirstOrDefaultAsync(pe => pe.Id == proposalEmployeeId && pe.ProposalId == proposalId && pe.Active, cancellationToken);
        if (item is null)
        {
            return OperationResult<bool>.Fail("not_found", "Proposal employee not found.");
        }

        item.Active = false;
        item.UpdatedAt = DateTime.UtcNow;

        await RecalculateProposalTotalsAsync(proposalId, cancellationToken);
        proposal.UpdatedAt = DateTime.UtcNow;

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

    private async Task<OperationResult<decimal>> ResolveEmployeeCostSnapshotAsync(int employeeId, CancellationToken cancellationToken)
    {
        var contract = await _context.EmployeeContracts
            .AsNoTracking()
            .Where(c => c.EmployeeId == employeeId && c.Active)
            .OrderByDescending(c => c.StartDate)
            .ThenByDescending(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (contract is null)
        {
            return OperationResult<decimal>.Fail("domain_error", "Employee has no active contract.");
        }

        if (!contract.BaseSalaryUsd.HasValue)
        {
            return OperationResult<decimal>.Fail("domain_error", "Employee active contract has no base salary.");
        }

        var cost = contract.BaseSalaryUsd.Value;

        var benefits = await _context.EmployeeContractBenefits
            .AsNoTracking()
            .Where(b => b.ContractId == contract.Id && b.Active)
            .ToListAsync(cancellationToken);

        foreach (var benefit in benefits)
        {
            if (!benefit.IsFormula)
            {
                cost += benefit.Value ?? 0m;
                continue;
            }

            var formula = benefit.Formula?.Trim();
            if (string.IsNullOrWhiteSpace(formula))
            {
                return OperationResult<decimal>.Fail("domain_error", "Active formula benefit has no formula.");
            }

            var variablesResult = await _formulaResolver.ResolveAsync(contract.Id, formula, cancellationToken);
            if (!variablesResult.Success)
            {
                return OperationResult<decimal>.Fail("domain_error", variablesResult.ErrorMessage ?? "Unable to resolve formula variables.");
            }

            if (!_formulaEvaluator.TryEvaluate(formula, variablesResult.Data!, out var formulaValue, out var errorMessage))
            {
                return OperationResult<decimal>.Fail("domain_error", errorMessage ?? "Unable to evaluate benefit formula.");
            }

            cost += formulaValue;
        }

        return OperationResult<decimal>.Ok(RoundCurrency(cost));
    }

    private async Task RecalculateProposalTotalsAsync(int proposalId, CancellationToken cancellationToken)
    {
        var proposal = await _context.Proposals.FirstAsync(p => p.Id == proposalId, cancellationToken);

        var totals = await _context.ProposalEmployees
            .AsNoTracking()
            .Where(pe => pe.ProposalId == proposalId && pe.Active)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalCost = g.Sum(x => x.CostSnapshot),
                TotalSellPrice = g.Sum(x => x.SellPriceSnapshot)
            })
            .FirstOrDefaultAsync(cancellationToken);

        proposal.TotalCost = RoundCurrency(totals?.TotalCost ?? 0m);
        proposal.TotalSellPrice = RoundCurrency(totals?.TotalSellPrice ?? 0m);
    }

    private static string? NormalizeStatus(string? status)
        => status?.Trim();

    private static ProposalDto Map(Proposal entity)
        => new(
            entity.Id,
            entity.ClientId,
            entity.Client?.Name ?? string.Empty,
            entity.OpportunityId,
            entity.Title,
            entity.ObjectiveHtml,
            entity.ProjectHours,
            entity.GlobalMarginPercent,
            entity.Status,
            entity.TotalCost,
            entity.TotalSellPrice,
            entity.Active,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static decimal RoundCurrency(decimal value)
        => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal RoundHourly(decimal value)
        => decimal.Round(value, 4, MidpointRounding.AwayFromZero);

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
