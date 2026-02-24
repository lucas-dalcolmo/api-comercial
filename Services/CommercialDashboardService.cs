using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace Api.Comercial.Services;

public interface ICommercialDashboardService
{
    Task<OperationResult<CommercialDashboardDto>> GetAsync(CancellationToken cancellationToken);
}

public sealed class CommercialDashboardService : ICommercialDashboardService
{
    private readonly ApeironDbContext _context;

    public CommercialDashboardService(ApeironDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<CommercialDashboardDto>> GetAsync(CancellationToken cancellationToken)
    {
        var activeOpportunities = await _context.Opportunities
            .AsNoTracking()
            .CountAsync(o => o.Active, cancellationToken);

        var openProposals = await _context.Proposals
            .AsNoTracking()
            .CountAsync(
                p => p.Active
                    && (p.Status == ProposalStatus.Draft || p.Status == ProposalStatus.Sent),
                cancellationToken);

        var activeClients = await _context.Clients
            .AsNoTracking()
            .CountAsync(c => c.Active, cancellationToken);

        var totalPipelineValue = await _context.Opportunities
            .AsNoTracking()
            .Where(o => o.Active)
            .Select(o => (decimal?)o.EstimatedValue)
            .SumAsync(cancellationToken);

        var averageProbability = await _context.Opportunities
            .AsNoTracking()
            .Where(o => o.Active && o.ProbabilityPercent.HasValue)
            .Select(o => o.ProbabilityPercent)
            .AverageAsync(cancellationToken);

        var recentOpportunities = await _context.Opportunities
            .AsNoTracking()
            .Where(o => o.Active)
            .OrderByDescending(o => o.UpdatedAt)
            .ThenByDescending(o => o.Id)
            .Take(5)
            .Select(o => new CommercialDashboardOpportunityDto(
                o.Id,
                o.Client != null ? o.Client.Name : "-",
                o.Name,
                o.FunnelStage != null ? o.FunnelStage.Name : "-",
                o.EstimatedValue,
                o.CurrencyCode,
                o.Status != null ? o.Status.Name : "-"))
            .ToListAsync(cancellationToken);

        var dto = new CommercialDashboardDto(
            activeOpportunities,
            openProposals,
            activeClients,
            totalPipelineValue ?? 0m,
            averageProbability,
            recentOpportunities);

        return OperationResult<CommercialDashboardDto>.Ok(dto);
    }
}
