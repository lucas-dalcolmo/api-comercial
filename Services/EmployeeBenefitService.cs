using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;

namespace Api.Comercial.Services;

public interface IEmployeeBenefitService
{
    Task<OperationResult<EmployeeBenefitDto>> GetByIdAsync(int employeeId, int benefitId, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<EmployeeBenefitDto>>> GetAllAsync(EmployeeBenefitQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeBenefitDto>> CreateAsync(int employeeId, EmployeeBenefitCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeBenefitDto>> PatchAsync(int employeeId, int benefitId, EmployeeBenefitUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(int employeeId, int benefitId, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeBenefitDto>> ReactivateAsync(int employeeId, int benefitId, CancellationToken cancellationToken);
}

public sealed class EmployeeBenefitService : IEmployeeBenefitService
{
    private readonly ApeironDbContext _context;

    public EmployeeBenefitService(ApeironDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<EmployeeBenefitDto>> GetByIdAsync(int employeeId, int benefitId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeBenefits
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.Id == benefitId, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeBenefitDto>.Fail("not_found", "Record not found.");
        }

        return OperationResult<EmployeeBenefitDto>.Ok(MapBenefit(entity));
    }

    public async Task<OperationResult<PagedResult<EmployeeBenefitDto>>> GetAllAsync(EmployeeBenefitQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dataQuery = _context.EmployeeBenefits.AsNoTracking().AsQueryable();

        if (query.EmployeeId.HasValue)
        {
            dataQuery = dataQuery.Where(e => e.EmployeeId == query.EmployeeId.Value);
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
        var items = await dataQuery
            .OrderByDescending(e => e.StartDate)
            .ThenByDescending(e => e.Id)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(e => MapBenefit(e))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<EmployeeBenefitDto>(items, currentPage, pageSize, totalCount);
        return OperationResult<PagedResult<EmployeeBenefitDto>>.Ok(result);
    }

    public async Task<OperationResult<EmployeeBenefitDto>> CreateAsync(int employeeId, EmployeeBenefitCreateDto dto, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
        if (employee is null || !employee.Active)
        {
            return OperationResult<EmployeeBenefitDto>.Fail("not_found", "Employee not found.");
        }

        var entity = new EmployeeBenefit
        {
            EmployeeId = employeeId,
            BenefitTypeId = dto.BenefitTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Active = true
        };

        _context.EmployeeBenefits.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeBenefitDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<EmployeeBenefitDto>.Ok(MapBenefit(entity));
    }

    public async Task<OperationResult<EmployeeBenefitDto>> PatchAsync(int employeeId, int benefitId, EmployeeBenefitUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeBenefits
            .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.Id == benefitId, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeBenefitDto>.Fail("not_found", "Record not found.");
        }

        if (dto.BenefitTypeId is null
            && dto.StartDate is null
            && dto.EndDate is null
            && !dto.Active.HasValue)
        {
            return OperationResult<EmployeeBenefitDto>.Fail("validation", "At least one field must be provided.");
        }

        if (dto.BenefitTypeId.HasValue) entity.BenefitTypeId = dto.BenefitTypeId.Value;
        if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) entity.EndDate = dto.EndDate.Value;
        if (dto.Active.HasValue) entity.Active = dto.Active.Value;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeBenefitDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<EmployeeBenefitDto>.Ok(MapBenefit(entity));
    }

    public async Task<OperationResult<bool>> DeleteAsync(int employeeId, int benefitId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeBenefits
            .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.Id == benefitId, cancellationToken);

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

    public async Task<OperationResult<EmployeeBenefitDto>> ReactivateAsync(int employeeId, int benefitId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeBenefits
            .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.Id == benefitId, cancellationToken);

        if (entity is null)
        {
            return OperationResult<EmployeeBenefitDto>.Fail("not_found", "Record not found.");
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
                return OperationResult<EmployeeBenefitDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
            }
        }

        return OperationResult<EmployeeBenefitDto>.Ok(MapBenefit(entity));
    }

    private static EmployeeBenefitDto MapBenefit(EmployeeBenefit entity)
        => new(
            entity.Id,
            entity.EmployeeId,
            entity.BenefitTypeId,
            entity.StartDate,
            entity.EndDate,
            entity.Active);

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
