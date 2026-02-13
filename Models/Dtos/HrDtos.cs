namespace Api.Comercial.Models.Dtos;

public sealed record EmployeeCreateDto(
    string FullName,
    string? Cpf,
    int? GenderId,
    DateTime? BirthDate,
    string? Nationality,
    string? PlaceOfBirth,
    int? MaritalStatusId,
    int? ChildrenCount,
    string? Phone,
    string? PersonalEmail,
    string? CorporateEmail,
    string? Address,
    int? EducationLevelId,
    int? BloodTypeId,
    DateTime? HireDate,
    EmployeeContractCreateDto? Contract);

public sealed record EmployeeUpdateDto(
    string? FullName,
    string? Cpf,
    int? GenderId,
    DateTime? BirthDate,
    string? Nationality,
    string? PlaceOfBirth,
    int? MaritalStatusId,
    int? ChildrenCount,
    string? Phone,
    string? PersonalEmail,
    string? CorporateEmail,
    string? Address,
    int? EducationLevelId,
    int? BloodTypeId,
    DateTime? HireDate,
    bool? Active);

public sealed record EmployeeDto(
    int Id,
    string FullName,
    string? Cpf,
    int? GenderId,
    DateTime? BirthDate,
    string? Nationality,
    string? PlaceOfBirth,
    int? MaritalStatusId,
    int? ChildrenCount,
    string? Phone,
    string? PersonalEmail,
    string? CorporateEmail,
    string? Address,
    int? EducationLevelId,
    int? BloodTypeId,
    DateTime? HireDate,
    bool Active,
    EmployeeContractDto? CurrentContract);

public sealed record EmployeeQueryDto(
    string? FullName,
    string? Cpf,
    bool? Active,
    int? Page,
    int? PageSize);

public sealed record EmployeeContractCreateDto(
    int? EmploymentTypeId,
    string? Cnpj,
    int? RoleId,
    int? DepartmentId,
    int? RegionId,
    int? OfficeId,
    decimal? BaseSalaryUsd,
    DateTime? StartDate,
    DateTime? EndDate);

public sealed record EmployeeContractCreateGlobalDto(
    int? EmployeeId,
    int? EmploymentTypeId,
    string? Cnpj,
    int? RoleId,
    int? DepartmentId,
    int? RegionId,
    int? OfficeId,
    decimal? BaseSalaryUsd,
    DateTime? StartDate,
    DateTime? EndDate);

public sealed record EmployeeContractUpdateGlobalDto(
    int? EmployeeId,
    int? EmploymentTypeId,
    string? Cnpj,
    int? RoleId,
    int? DepartmentId,
    int? RegionId,
    int? OfficeId,
    decimal? BaseSalaryUsd,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? Active);

public sealed record EmployeeContractUpdateDto(
    int? EmploymentTypeId,
    string? Cnpj,
    int? RoleId,
    int? DepartmentId,
    int? RegionId,
    int? OfficeId,
    decimal? BaseSalaryUsd,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? Active);

public sealed record EmployeeContractDto(
    int Id,
    int? EmployeeId,
    int? EmploymentTypeId,
    string? Cnpj,
    int? RoleId,
    int? DepartmentId,
    int? RegionId,
    int? OfficeId,
    decimal? BaseSalaryUsd,
    DateTime? StartDate,
    DateTime? EndDate,
    bool Active);

public sealed record EmployeeContractListDto(
    int Id,
    int? EmployeeId,
    string? EmployeeFullName,
    int? EmploymentTypeId,
    string? Cnpj,
    int? RoleId,
    int? DepartmentId,
    int? RegionId,
    int? OfficeId,
    decimal? BaseSalaryUsd,
    DateTime? StartDate,
    DateTime? EndDate,
    bool Active);

public sealed record EmployeeContractQueryDto(
    int? EmployeeId,
    bool? Active,
    int? Page,
    int? PageSize);

public sealed record EmployeeDocumentCreateDto(
    int? DocumentTypeId,
    string? DocumentNumber,
    string? CountryCode,
    DateTime? IssueDate,
    DateTime? ExpiryDate,
    string? Notes);

public sealed record EmployeeDocumentUpdateDto(
    int? DocumentTypeId,
    string? DocumentNumber,
    string? CountryCode,
    DateTime? IssueDate,
    DateTime? ExpiryDate,
    string? Notes,
    bool? Active);

public sealed record EmployeeDocumentDto(
    int Id,
    int EmployeeId,
    int? DocumentTypeId,
    string? DocumentNumber,
    string? CountryCode,
    DateTime? IssueDate,
    DateTime? ExpiryDate,
    string? Notes,
    bool Active);

public sealed record EmployeeDocumentQueryDto(
    int? EmployeeId,
    int? DocumentTypeId,
    bool? Active,
    int? Page,
    int? PageSize);

public sealed record EmployeeBenefitCreateDto(
    int? BenefitTypeId,
    DateTime? StartDate,
    DateTime? EndDate);

public sealed record EmployeeBenefitUpdateDto(
    int? BenefitTypeId,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? Active);

public sealed record EmployeeBenefitDto(
    int Id,
    int EmployeeId,
    int? BenefitTypeId,
    DateTime? StartDate,
    DateTime? EndDate,
    bool Active);

public sealed record EmployeeBenefitQueryDto(
    int? EmployeeId,
    int? BenefitTypeId,
    bool? Active,
    int? Page,
    int? PageSize);

/// <summary>
/// Request payload for creating a contract benefit.
/// Fixed example: {"benefitTypeId":3,"value":450.00,"isFormula":false,"formula":null}
/// Formula example: {"benefitTypeId":3,"value":null,"isFormula":true,"formula":"0.30 * [EmployeeContract.BaseSalaryUsd]"}
/// </summary>
public sealed record EmployeeContractBenefitCreateDto(
    int? BenefitTypeId,
    decimal? Value,
    bool IsFormula,
    string? Formula);

/// <summary>
/// Request payload for updating a contract benefit.
/// Formula update example: {"isFormula":true,"value":null,"formula":"0.30 * [EmployeeContract.BaseSalaryUsd]","active":true}
/// </summary>
public sealed record EmployeeContractBenefitUpdateDto(
    int? BenefitTypeId,
    decimal? Value,
    bool? IsFormula,
    string? Formula,
    bool? Active);

/// <summary>
/// Response payload for contract benefits.
/// Fixed response example: {"id":11,"contractId":5,"benefitTypeId":3,"value":450.00,"isFormula":false,"formula":null,"calculatedValue":450.00,"active":true}
/// Formula response example: {"id":12,"contractId":5,"benefitTypeId":3,"value":null,"isFormula":true,"formula":"0.30 * [EmployeeContract.BaseSalaryUsd]","calculatedValue":1050.00,"active":true}
/// </summary>
public sealed record EmployeeContractBenefitDto(
    int Id,
    int ContractId,
    int? BenefitTypeId,
    decimal? Value,
    bool IsFormula,
    string? Formula,
    decimal CalculatedValue,
    bool Active);

public sealed record EmployeeContractBenefitQueryDto(
    int? ContractId,
    int? BenefitTypeId,
    bool? Active,
    int? Page,
    int? PageSize);

public sealed record BenefitFormulaVariableCreateDto(
    string VariableKey,
    string SourceScope,
    string SourceSchema,
    string SourceTable,
    string SourceColumn);

public sealed record BenefitFormulaVariableUpdateDto(
    string? VariableKey,
    string? SourceScope,
    string? SourceSchema,
    string? SourceTable,
    string? SourceColumn,
    bool? Active);

public sealed record BenefitFormulaVariableDto(
    int Id,
    string VariableKey,
    string SourceScope,
    string SourceSchema,
    string SourceTable,
    string SourceColumn,
    bool Active);

public sealed record BenefitFormulaVariableQueryDto(
    int? Id,
    string? VariableKey,
    string? SourceScope,
    string? SourceSchema,
    string? SourceTable,
    string? SourceColumn,
    bool? Active,
    int? Page,
    int? PageSize);
