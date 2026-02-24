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
        var currentYear = DateTime.UtcNow.Year;

        var openPipelineOpportunities = _context.Opportunities
            .AsNoTracking()
            .Where(o => o.Active)
            .Where(o =>
                o.Status == null
                || string.IsNullOrWhiteSpace(o.Status.Name)
                || (
                    !o.Status.Name.ToLower().Contains("closed")
                    && !o.Status.Name.ToLower().Contains("won")
                    && !o.Status.Name.ToLower().Contains("lost")
                    && !o.Status.Name.ToLower().Contains("cancel")
                    && !o.Status.Name.ToLower().Contains("perd")
                ));

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

        var totalPipelineValue = await openPipelineOpportunities
            .Select(o => (decimal?)o.EstimatedValue)
            .SumAsync(cancellationToken);

        var probabilitySource = await openPipelineOpportunities
            .Where(o => o.ProbabilityPercent.HasValue)
            .Select(o => new
            {
                Probability = o.ProbabilityPercent!.Value,
                EstimatedValue = o.EstimatedValue
            })
            .ToListAsync(cancellationToken);

        decimal? averageProbability = null;
        var weightedSource = probabilitySource
            .Where(x => x.EstimatedValue.HasValue && x.EstimatedValue.Value > 0)
            .ToList();

        if (weightedSource.Count > 0)
        {
            var weightedDenominator = weightedSource.Sum(x => x.EstimatedValue!.Value);
            if (weightedDenominator > 0)
            {
                var weightedNumerator = weightedSource
                    .Sum(x => x.EstimatedValue!.Value * x.Probability);
                averageProbability = decimal.Round(weightedNumerator / weightedDenominator, 4);
            }
        }
        else if (probabilitySource.Count > 0)
        {
            averageProbability = decimal.Round(probabilitySource.Average(x => x.Probability), 4);
        }

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

        var forecastSource = await openPipelineOpportunities
            .Where(o =>
                o.EstimatedValue.HasValue
                && o.ProbabilityPercent.HasValue
                && (o.ForecastDate ?? o.UpdatedAt).Year == currentYear)
            .Select(o => new
            {
                Date = o.ForecastDate ?? o.UpdatedAt,
                WeightedValue = o.EstimatedValue!.Value * (o.ProbabilityPercent!.Value / 100m)
            })
            .ToListAsync(cancellationToken);

        var quarterlyForecast = Enumerable.Range(1, 4)
            .Select(quarter =>
            {
                var quarterValue = forecastSource
                    .Where(x => ((x.Date.Month - 1) / 3) + 1 == quarter)
                    .Sum(x => x.WeightedValue);
                return new CommercialDashboardForecastBucketDto($"Q{quarter}", decimal.Round(quarterValue, 2));
            })
            .ToList();

        var semesterForecast = new List<CommercialDashboardForecastBucketDto>
        {
            new(
                "H1",
                decimal.Round(
                    forecastSource
                        .Where(x => x.Date.Month >= 1 && x.Date.Month <= 6)
                        .Sum(x => x.WeightedValue),
                    2)),
            new(
                "H2",
                decimal.Round(
                    forecastSource
                        .Where(x => x.Date.Month >= 7 && x.Date.Month <= 12)
                        .Sum(x => x.WeightedValue),
                    2))
        };

        var dto = new CommercialDashboardDto(
            activeOpportunities,
            openProposals,
            activeClients,
            totalPipelineValue ?? 0m,
            averageProbability,
            recentOpportunities,
            quarterlyForecast,
            semesterForecast);

        return OperationResult<CommercialDashboardDto>.Ok(dto);
    }
}
