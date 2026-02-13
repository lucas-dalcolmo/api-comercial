namespace Api.Comercial.Models.Entities;

public sealed class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public int? GenderId { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Nationality { get; set; }
    public string? PlaceOfBirth { get; set; }
    public int? MaritalStatusId { get; set; }
    public int? ChildrenCount { get; set; }
    public string? Phone { get; set; }
    public string? PersonalEmail { get; set; }
    public string? CorporateEmail { get; set; }
    public string? Address { get; set; }
    public int? EducationLevelId { get; set; }
    public int? BloodTypeId { get; set; }
    public DateTime? HireDate { get; set; }
    public bool Active { get; set; }

    public ICollection<EmployeeContract> Contracts { get; set; } = new List<EmployeeContract>();
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public ICollection<EmployeeBenefit> Benefits { get; set; } = new List<EmployeeBenefit>();
}

public sealed class EmployeeContract
{
    public int Id { get; set; }
    public int? EmployeeId { get; set; }
    public int? EmploymentTypeId { get; set; }
    public string? Cnpj { get; set; }
    public int? RoleId { get; set; }
    public int? DepartmentId { get; set; }
    public int? RegionId { get; set; }
    public int? OfficeId { get; set; }
    public decimal? BaseSalaryUsd { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool Active { get; set; }

    public Employee? Employee { get; set; }
    public ICollection<EmployeeContractBenefit> Benefits { get; set; } = new List<EmployeeContractBenefit>();
}

public sealed class EmployeeDocument
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int? DocumentTypeId { get; set; }
    public string? DocumentNumber { get; set; }
    public string? CountryCode { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public bool Active { get; set; }

    public Employee? Employee { get; set; }
}

public sealed class EmployeeBenefit
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int? BenefitTypeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool Active { get; set; }

    public Employee? Employee { get; set; }
}

public sealed class EmployeeContractBenefit
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int? BenefitTypeId { get; set; }
    public decimal? Value { get; set; }
    public bool IsFormula { get; set; }
    public string? Formula { get; set; }
    public bool Active { get; set; }

    public EmployeeContract? Contract { get; set; }
}

public sealed class BenefitFormulaVariable
{
    public int Id { get; set; }
    public string VariableKey { get; set; } = string.Empty;
    public string SourceScope { get; set; } = string.Empty;
    public string SourceSchema { get; set; } = string.Empty;
    public string SourceTable { get; set; } = string.Empty;
    public string SourceColumn { get; set; } = string.Empty;
    public bool Active { get; set; }
}
