using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;

namespace Api.Comercial.Services;

public interface IEmployeeService
{
    Task<OperationResult<EmployeeDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<EmployeeDto>>> GetAllAsync(EmployeeQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeDto>> CreateAsync(EmployeeCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeDto>> PatchAsync(int id, EmployeeUpdateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<EmployeeDto>> ReactivateAsync(int id, CancellationToken cancellationToken);
}

public sealed class EmployeeService : IEmployeeService
{
    private readonly ApeironDbContext _context;

    public EmployeeService(ApeironDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<EmployeeDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.Employees
            .Include(e => e.Contracts)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeDto>.Fail("not_found", "Record not found.");
        }

        return OperationResult<EmployeeDto>.Ok(MapEmployee(entity));
    }

    public async Task<OperationResult<PagedResult<EmployeeDto>>> GetAllAsync(EmployeeQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);
        var name = query.FullName?.Trim();
        var cpf = query.Cpf?.Trim();

        var dataQuery = _context.Employees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            dataQuery = dataQuery.Where(e => e.FullName.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(cpf))
        {
            dataQuery = dataQuery.Where(e => e.Cpf == cpf);
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
            .OrderBy(e => e.FullName)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeDto(
                e.Id,
                e.FullName,
                e.Cpf,
                e.GenderId,
                e.BirthDate,
                e.Nationality,
                e.PlaceOfBirth,
                e.MaritalStatusId,
                e.ChildrenCount,
                e.Phone,
                e.PersonalEmail,
                e.CorporateEmail,
                e.Address,
                e.EducationLevelId,
                e.BloodTypeId,
                e.HireDate,
                e.Active,
                _context.EmployeeContracts
                    .Where(c => c.EmployeeId == e.Id && c.Active)
                    .OrderByDescending(c => c.StartDate)
                    .ThenByDescending(c => c.Id)
                    .Select(c => new EmployeeContractDto(
                        c.Id,
                        c.EmployeeId,
                        c.EmploymentTypeId,
                        c.Cnpj,
                        c.RoleId,
                        c.DepartmentId,
                        c.RegionId,
                        c.OfficeId,
                        c.BaseSalaryUsd,
                        c.StartDate,
                        c.EndDate,
                        c.Active))
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<EmployeeDto>(items, currentPage, pageSize, totalCount);
        return OperationResult<PagedResult<EmployeeDto>>.Ok(result);
    }

    public async Task<OperationResult<EmployeeDto>> CreateAsync(EmployeeCreateDto dto, CancellationToken cancellationToken)
    {
        var name = dto.FullName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<EmployeeDto>.Fail("validation", "FullName is required.");
        }

        var entity = new Employee
        {
            FullName = name,
            Cpf = dto.Cpf?.Trim(),
            GenderId = dto.GenderId,
            BirthDate = dto.BirthDate,
            Nationality = dto.Nationality?.Trim(),
            PlaceOfBirth = dto.PlaceOfBirth?.Trim(),
            MaritalStatusId = dto.MaritalStatusId,
            ChildrenCount = dto.ChildrenCount,
            Phone = dto.Phone?.Trim(),
            PersonalEmail = dto.PersonalEmail?.Trim(),
            CorporateEmail = dto.CorporateEmail?.Trim(),
            Address = dto.Address?.Trim(),
            EducationLevelId = dto.EducationLevelId,
            BloodTypeId = dto.BloodTypeId,
            HireDate = dto.HireDate,
            Active = true
        };

        _context.Employees.Add(entity);

        if (dto.Contract is not null)
        {
            var contract = new EmployeeContract
            {
                Employee = entity,
                EmploymentTypeId = dto.Contract.EmploymentTypeId,
                Cnpj = dto.Contract.Cnpj?.Trim(),
                RoleId = dto.Contract.RoleId,
                DepartmentId = dto.Contract.DepartmentId,
                RegionId = dto.Contract.RegionId,
                OfficeId = dto.Contract.OfficeId,
                BaseSalaryUsd = dto.Contract.BaseSalaryUsd,
                StartDate = dto.Contract.StartDate,
                EndDate = dto.Contract.EndDate,
                Active = true
            };

            _context.EmployeeContracts.Add(contract);
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<EmployeeDto>.Ok(MapEmployee(entity));
    }

    public async Task<OperationResult<EmployeeDto>> PatchAsync(int id, EmployeeUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.Employees
            .Include(e => e.Contracts)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null || !entity.Active)
        {
            return OperationResult<EmployeeDto>.Fail("not_found", "Record not found.");
        }

        if (dto.FullName is null
            && dto.Cpf is null
            && dto.GenderId is null
            && dto.BirthDate is null
            && dto.Nationality is null
            && dto.PlaceOfBirth is null
            && dto.MaritalStatusId is null
            && dto.ChildrenCount is null
            && dto.Phone is null
            && dto.PersonalEmail is null
            && dto.CorporateEmail is null
            && dto.Address is null
            && dto.EducationLevelId is null
            && dto.BloodTypeId is null
            && dto.HireDate is null
            && !dto.Active.HasValue)
        {
            return OperationResult<EmployeeDto>.Fail("validation", "At least one field must be provided.");
        }

        if (dto.FullName is not null)
        {
            var name = dto.FullName.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return OperationResult<EmployeeDto>.Fail("validation", "FullName is required.");
            }

            entity.FullName = name;
        }

        if (dto.Cpf is not null) entity.Cpf = dto.Cpf.Trim();
        if (dto.GenderId.HasValue) entity.GenderId = dto.GenderId.Value;
        if (dto.BirthDate.HasValue) entity.BirthDate = dto.BirthDate.Value;
        if (dto.Nationality is not null) entity.Nationality = dto.Nationality.Trim();
        if (dto.PlaceOfBirth is not null) entity.PlaceOfBirth = dto.PlaceOfBirth.Trim();
        if (dto.MaritalStatusId.HasValue) entity.MaritalStatusId = dto.MaritalStatusId.Value;
        if (dto.ChildrenCount.HasValue) entity.ChildrenCount = dto.ChildrenCount.Value;
        if (dto.Phone is not null) entity.Phone = dto.Phone.Trim();
        if (dto.PersonalEmail is not null) entity.PersonalEmail = dto.PersonalEmail.Trim();
        if (dto.CorporateEmail is not null) entity.CorporateEmail = dto.CorporateEmail.Trim();
        if (dto.Address is not null) entity.Address = dto.Address.Trim();
        if (dto.EducationLevelId.HasValue) entity.EducationLevelId = dto.EducationLevelId.Value;
        if (dto.BloodTypeId.HasValue) entity.BloodTypeId = dto.BloodTypeId.Value;
        if (dto.HireDate.HasValue) entity.HireDate = dto.HireDate.Value;
        if (dto.Active.HasValue) entity.Active = dto.Active.Value;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<EmployeeDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        return OperationResult<EmployeeDto>.Ok(MapEmployee(entity));
    }

    public async Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
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

    public async Task<OperationResult<EmployeeDto>> ReactivateAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.Employees
            .Include(e => e.Contracts)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null)
        {
            return OperationResult<EmployeeDto>.Fail("not_found", "Record not found.");
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
                return OperationResult<EmployeeDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
            }
        }

        return OperationResult<EmployeeDto>.Ok(MapEmployee(entity));
    }

    private static EmployeeDto MapEmployee(Employee entity)
    {
        var current = entity.Contracts
            .Where(c => c.Active)
            .OrderByDescending(c => c.StartDate)
            .ThenByDescending(c => c.Id)
            .FirstOrDefault();

        var contractDto = current is null
            ? null
            : new EmployeeContractDto(
                current.Id,
                current.EmployeeId,
                current.EmploymentTypeId,
                current.Cnpj,
                current.RoleId,
                current.DepartmentId,
                current.RegionId,
                current.OfficeId,
                current.BaseSalaryUsd,
                current.StartDate,
                current.EndDate,
                current.Active);

        return new EmployeeDto(
            entity.Id,
            entity.FullName,
            entity.Cpf,
            entity.GenderId,
            entity.BirthDate,
            entity.Nationality,
            entity.PlaceOfBirth,
            entity.MaritalStatusId,
            entity.ChildrenCount,
            entity.Phone,
            entity.PersonalEmail,
            entity.CorporateEmail,
            entity.Address,
            entity.EducationLevelId,
            entity.BloodTypeId,
            entity.HireDate,
            entity.Active,
            contractDto);
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
