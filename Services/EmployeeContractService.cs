using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;

namespace Api.Comercial.Services;

public interface IEmployeeContractService
{
    Task<OperationResult<EmployeeContractListDto>> GetByIdGlobalAsync(int contractId, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<EmployeeContractListDto>>> GetAllGlobalAsync(EmployeeContractQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeContractListDto>> CreateGlobalAsync(EmployeeContractCreateGlobalDto dto, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeContractListDto>> PatchGlobalAsync(int contractId, EmployeeContractUpdateGlobalDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteGlobalAsync(int contractId, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeContractListDto>> ReactivateGlobalAsync(int contractId, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeContractDto>> GetByIdAsync(int employeeId, int contractId, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<EmployeeContractDto>>> GetAllAsync(EmployeeContractQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeContractDto>> CreateAsync(int employeeId, EmployeeContractCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeContractDto>> PatchAsync(int employeeId, int contractId, EmployeeContractUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(int employeeId, int contractId, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeContractDto>> ReactivateAsync(int employeeId, int contractId, CancellationToken cancellationToken);
}

public sealed class EmployeeContractService : IEmployeeContractService
{
    private readonly ApeironDbContext _context;

    public EmployeeContractService(ApeironDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<EmployeeContractListDto>> GetByIdGlobalAsync(int contractId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContracts
            .AsNoTracking()
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeContractListDto>.Fail("not_found", "Record not found.");
        }

        return OperationResult<EmployeeContractListDto>.Ok(MapContractList(entity));
    }

    public async Task<OperationResult<PagedResult<EmployeeContractListDto>>> GetAllGlobalAsync(EmployeeContractQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dataQuery = _context.EmployeeContracts
            .AsNoTracking()
            .Include(c => c.Employee)
            .AsQueryable();

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
            .Select(e => MapContractList(e))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<EmployeeContractListDto>(items, currentPage, pageSize, totalCount);
        return OperationResult<PagedResult<EmployeeContractListDto>>.Ok(result);
    }

    public async Task<OperationResult<EmployeeContractListDto>> CreateGlobalAsync(EmployeeContractCreateGlobalDto dto, CancellationToken cancellationToken)
    {
        Employee? employee = null;
        if (dto.EmployeeId.HasValue)
        {
            employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == dto.EmployeeId.Value, cancellationToken);
            if (employee is null || !employee.Active)
            {
                return OperationResult<EmployeeContractListDto>.Fail("not_found", "Employee not found.");
            }
        }

        var entity = new EmployeeContract
        {
            EmployeeId = dto.EmployeeId,
            EmploymentTypeId = dto.EmploymentTypeId,
            Cnpj = dto.Cnpj?.Trim(),
            RoleId = dto.RoleId,
            DepartmentId = dto.DepartmentId,
            RegionId = dto.RegionId,
            OfficeId = dto.OfficeId,
            BaseSalaryUsd = dto.BaseSalaryUsd,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Active = true
        };

        _context.EmployeeContracts.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeContractListDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        entity.Employee = employee;
        return OperationResult<EmployeeContractListDto>.Ok(MapContractList(entity));
    }

    public async Task<OperationResult<EmployeeContractListDto>> PatchGlobalAsync(int contractId, EmployeeContractUpdateGlobalDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContracts
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeContractListDto>.Fail("not_found", "Record not found.");
        }

        if (dto.EmployeeId is null
            && dto.EmploymentTypeId is null
            && dto.Cnpj is null
            && dto.RoleId is null
            && dto.DepartmentId is null
            && dto.RegionId is null
            && dto.OfficeId is null
            && dto.BaseSalaryUsd is null
            && dto.StartDate is null
            && dto.EndDate is null
            && !dto.Active.HasValue)
        {
            return OperationResult<EmployeeContractListDto>.Fail("validation", "At least one field must be provided.");
        }

        if (dto.EmployeeId.HasValue)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == dto.EmployeeId.Value, cancellationToken);
            if (employee is null || !employee.Active)
            {
                return OperationResult<EmployeeContractListDto>.Fail("not_found", "Employee not found.");
            }

            entity.EmployeeId = dto.EmployeeId.Value;
            entity.Employee = employee;
        }
        if (dto.EmploymentTypeId.HasValue) entity.EmploymentTypeId = dto.EmploymentTypeId.Value;
        if (dto.Cnpj is not null) entity.Cnpj = dto.Cnpj.Trim();
        if (dto.RoleId.HasValue) entity.RoleId = dto.RoleId.Value;
        if (dto.DepartmentId.HasValue) entity.DepartmentId = dto.DepartmentId.Value;
        if (dto.RegionId.HasValue) entity.RegionId = dto.RegionId.Value;
        if (dto.OfficeId.HasValue) entity.OfficeId = dto.OfficeId.Value;
        if (dto.BaseSalaryUsd.HasValue) entity.BaseSalaryUsd = dto.BaseSalaryUsd.Value;
        if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) entity.EndDate = dto.EndDate.Value;
        if (dto.Active.HasValue) entity.Active = dto.Active.Value;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeContractListDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<EmployeeContractListDto>.Ok(MapContractList(entity));
    }

    public async Task<OperationResult<bool>> DeleteGlobalAsync(int contractId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContracts
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

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

    public async Task<OperationResult<EmployeeContractListDto>> ReactivateGlobalAsync(int contractId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContracts
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (entity is null)
        {
            return OperationResult<EmployeeContractListDto>.Fail("not_found", "Record not found.");
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
                return OperationResult<EmployeeContractListDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
            }
        }

        return OperationResult<EmployeeContractListDto>.Ok(MapContractList(entity));
    }

    public async Task<OperationResult<EmployeeContractDto>> GetByIdAsync(int employeeId, int contractId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContracts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.Id == contractId, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeContractDto>.Fail("not_found", "Record not found.");
        }

        return OperationResult<EmployeeContractDto>.Ok(MapContract(entity));
    }

    public async Task<OperationResult<PagedResult<EmployeeContractDto>>> GetAllAsync(EmployeeContractQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dataQuery = _context.EmployeeContracts.AsNoTracking().AsQueryable();

        if (query.EmployeeId.HasValue)
        {
            dataQuery = dataQuery.Where(e => e.EmployeeId == query.EmployeeId.Value);
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
            .Select(e => MapContract(e))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<EmployeeContractDto>(items, currentPage, pageSize, totalCount);
        return OperationResult<PagedResult<EmployeeContractDto>>.Ok(result);
    }

    public async Task<OperationResult<EmployeeContractDto>> CreateAsync(int employeeId, EmployeeContractCreateDto dto, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
        if (employee is null || !employee.Active)
        {
            return OperationResult<EmployeeContractDto>.Fail("not_found", "Employee not found.");
        }

        var entity = new EmployeeContract
        {
            EmployeeId = employeeId,
            EmploymentTypeId = dto.EmploymentTypeId,
            Cnpj = dto.Cnpj?.Trim(),
            RoleId = dto.RoleId,
            DepartmentId = dto.DepartmentId,
            RegionId = dto.RegionId,
            OfficeId = dto.OfficeId,
            BaseSalaryUsd = dto.BaseSalaryUsd,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Active = true
        };

        _context.EmployeeContracts.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeContractDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<EmployeeContractDto>.Ok(MapContract(entity));
    }

    public async Task<OperationResult<EmployeeContractDto>> PatchAsync(int employeeId, int contractId, EmployeeContractUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContracts
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.Id == contractId, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeContractDto>.Fail("not_found", "Record not found.");
        }

        if (dto.EmploymentTypeId is null
            && dto.Cnpj is null
            && dto.RoleId is null
            && dto.DepartmentId is null
            && dto.RegionId is null
            && dto.OfficeId is null
            && dto.BaseSalaryUsd is null
            && dto.StartDate is null
            && dto.EndDate is null
            && !dto.Active.HasValue)
        {
            return OperationResult<EmployeeContractDto>.Fail("validation", "At least one field must be provided.");
        }

        if (dto.EmploymentTypeId.HasValue) entity.EmploymentTypeId = dto.EmploymentTypeId.Value;
        if (dto.Cnpj is not null) entity.Cnpj = dto.Cnpj.Trim();
        if (dto.RoleId.HasValue) entity.RoleId = dto.RoleId.Value;
        if (dto.DepartmentId.HasValue) entity.DepartmentId = dto.DepartmentId.Value;
        if (dto.RegionId.HasValue) entity.RegionId = dto.RegionId.Value;
        if (dto.OfficeId.HasValue) entity.OfficeId = dto.OfficeId.Value;
        if (dto.BaseSalaryUsd.HasValue) entity.BaseSalaryUsd = dto.BaseSalaryUsd.Value;
        if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) entity.EndDate = dto.EndDate.Value;
        if (dto.Active.HasValue) entity.Active = dto.Active.Value;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeContractDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<EmployeeContractDto>.Ok(MapContract(entity));
    }

    public async Task<OperationResult<bool>> DeleteAsync(int employeeId, int contractId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContracts
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.Id == contractId, cancellationToken);

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

    public async Task<OperationResult<EmployeeContractDto>> ReactivateAsync(int employeeId, int contractId, CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeContracts
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.Id == contractId, cancellationToken);

        if (entity is null)
        {
            return OperationResult<EmployeeContractDto>.Fail("not_found", "Record not found.");
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
                return OperationResult<EmployeeContractDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
            }
        }

        return OperationResult<EmployeeContractDto>.Ok(MapContract(entity));
    }

    private static EmployeeContractDto MapContract(EmployeeContract entity)
        => new(
            entity.Id,
            entity.EmployeeId,
            entity.EmploymentTypeId,
            entity.Cnpj,
            entity.RoleId,
            entity.DepartmentId,
            entity.RegionId,
            entity.OfficeId,
            entity.BaseSalaryUsd,
            entity.StartDate,
            entity.EndDate,
            entity.Active);

    private static EmployeeContractListDto MapContractList(EmployeeContract entity)
        => new(
            entity.Id,
            entity.EmployeeId,
            entity.Employee?.FullName,
            entity.EmploymentTypeId,
            entity.Cnpj,
            entity.RoleId,
            entity.DepartmentId,
            entity.RegionId,
            entity.OfficeId,
            entity.BaseSalaryUsd,
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
