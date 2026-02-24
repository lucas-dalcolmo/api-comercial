using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace Api.Comercial.Services;

public interface IOperationalHrDashboardService
{
    Task<OperationResult<OperationalHrDashboardDto>> GetAsync(CancellationToken cancellationToken);
}

public sealed class OperationalHrDashboardService : IOperationalHrDashboardService
{
    private readonly ApeironDbContext _context;

    public OperationalHrDashboardService(ApeironDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<OperationalHrDashboardDto>> GetAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var next30 = today.AddDays(30);
        var next60 = today.AddDays(60);
        var next90 = today.AddDays(90);

        var activeEmployees = await _context.Employees
            .AsNoTracking()
            .CountAsync(e => e.Active, cancellationToken);

        var activeContractsQuery = _context.EmployeeContracts
            .AsNoTracking()
            .Where(c => c.Active && (c.EndDate == null || c.EndDate >= today));

        var activeContracts = await activeContractsQuery.CountAsync(cancellationToken);

        var contractsEndingIn30Days = await activeContractsQuery
            .CountAsync(c => c.EndDate != null && c.EndDate >= today && c.EndDate <= next30, cancellationToken);

        var documentsExpiringIn60Days = await _context.EmployeeDocuments
            .AsNoTracking()
            .Where(d =>
                d.Active
                && d.ExpiryDate != null
                && d.ExpiryDate >= today
                && d.ExpiryDate <= next60
                && _context.Employees.Any(e => e.Id == d.EmployeeId && e.Active))
            .CountAsync(cancellationToken);

        var monthlyBaseSalaryUsd = await activeContractsQuery
            .Select(c => (decimal?)c.BaseSalaryUsd)
            .SumAsync(cancellationToken) ?? 0m;

        var monthlyFixedBenefitsUsd = await (
            from b in _context.EmployeeContractBenefits.AsNoTracking()
            join c in _context.EmployeeContracts.AsNoTracking() on b.ContractId equals c.Id
            where b.Active
                && !b.IsFormula
                && c.Active
                && (c.EndDate == null || c.EndDate >= today)
            select (decimal?)b.Value
        ).SumAsync(cancellationToken) ?? 0m;

        var employmentTypeDistribution = await (
            from c in activeContractsQuery
            join et in _context.EmploymentTypes.AsNoTracking()
                on c.EmploymentTypeId equals et.Id into etg
            from et in etg.DefaultIfEmpty()
            group c by (et != null ? et.Name : "Unspecified") into g
            orderby g.Count() descending, g.Key
            select new OperationalHrEmploymentTypeItemDto(
                g.Key,
                g.Count())
        )
        .Take(8)
        .ToListAsync(cancellationToken);

        var upcomingContractExpirations = await (
            from c in activeContractsQuery
            where c.EndDate != null && c.EndDate >= today && c.EndDate <= next90
            join e in _context.Employees.AsNoTracking() on c.EmployeeId equals e.Id into eg
            from e in eg.DefaultIfEmpty()
            join et in _context.EmploymentTypes.AsNoTracking() on c.EmploymentTypeId equals et.Id into etg
            from et in etg.DefaultIfEmpty()
            join r in _context.Regions.AsNoTracking() on c.RegionId equals r.Id into rg
            from r in rg.DefaultIfEmpty()
            join o in _context.Offices.AsNoTracking() on c.OfficeId equals o.Id into og
            from o in og.DefaultIfEmpty()
            orderby c.EndDate, c.Id
            select new OperationalHrContractExpiryDto(
                c.Id,
                c.EmployeeId,
                e != null ? e.FullName : "Unassigned",
                c.EndDate,
                et != null ? et.Name : "Unspecified",
                r != null ? r.Name : "Unspecified",
                o != null ? o.Name : "Unspecified")
        )
        .Take(10)
        .ToListAsync(cancellationToken);

        var upcomingDocumentExpirations = await (
            from d in _context.EmployeeDocuments.AsNoTracking()
            where d.Active
                && d.ExpiryDate != null
                && d.ExpiryDate >= today
                && d.ExpiryDate <= next90
            join e in _context.Employees.AsNoTracking() on d.EmployeeId equals e.Id
            join dt in _context.DocumentTypes.AsNoTracking() on d.DocumentTypeId equals dt.Id into dtg
            from dt in dtg.DefaultIfEmpty()
            where e.Active
            orderby d.ExpiryDate, d.Id
            select new OperationalHrDocumentExpiryDto(
                d.Id,
                d.EmployeeId,
                e.FullName,
                dt != null ? dt.Name : "Unspecified",
                d.ExpiryDate)
        )
        .Take(10)
        .ToListAsync(cancellationToken);

        var dto = new OperationalHrDashboardDto(
            activeEmployees,
            activeContracts,
            contractsEndingIn30Days,
            documentsExpiringIn60Days,
            monthlyBaseSalaryUsd,
            monthlyFixedBenefitsUsd,
            employmentTypeDistribution,
            upcomingContractExpirations,
            upcomingDocumentExpirations);

        return OperationResult<OperationalHrDashboardDto>.Ok(dto);
    }
}
