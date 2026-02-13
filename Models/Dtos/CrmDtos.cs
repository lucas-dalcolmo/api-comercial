namespace Api.Comercial.Models.Dtos;

public sealed record ClientCreateDto(
    string Name,
    string? LegalName);

public sealed record ClientUpdateDto(
    string? Name,
    string? LegalName,
    bool? Active);

public sealed record ClientQueryDto(
    string? Name,
    bool? Active,
    int? Page,
    int? PageSize);

public sealed record ClientDto(
    int Id,
    string Name,
    string? LegalName,
    string? LogoUrl,
    bool Active);

public sealed record ProposalCreateDto(
    int ClientId,
    int? OpportunityId,
    string Title,
    string? ObjectiveHtml,
    decimal GlobalMarginPercent,
    string? Status);

public sealed record ProposalUpdateDto(
    int? ClientId,
    int? OpportunityId,
    string? Title,
    string? ObjectiveHtml,
    decimal? GlobalMarginPercent,
    string? Status,
    bool? Active);

public sealed record ProposalQueryDto(
    int? ClientId,
    string? Status,
    bool? Active,
    int? Page,
    int? PageSize);

public sealed record ProposalDto(
    int Id,
    int ClientId,
    string ClientName,
    int? OpportunityId,
    string Title,
    string ObjectiveHtml,
    decimal GlobalMarginPercent,
    string Status,
    decimal TotalCost,
    decimal TotalSellPrice,
    bool Active,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ProposalEmployeeAddDto(int EmployeeId);

public sealed record ProposalEmployeeDto(
    int Id,
    int ProposalId,
    int EmployeeId,
    string EmployeeName,
    decimal CostSnapshot,
    decimal MarginPercentApplied,
    decimal SellPriceSnapshot,
    bool Active,
    DateTime CreatedAt,
    DateTime UpdatedAt);
