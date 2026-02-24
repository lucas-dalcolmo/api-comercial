using Microsoft.AspNetCore.Http;

namespace Api.Comercial.Models.Dtos;

public sealed record ClientCreateDto(
    string Name,
    string? LegalName);

public sealed record ClientUpdateDto(
    string? Name,
    string? LegalName,
    bool? Active);

public sealed class ClientLogoUploadDto
{
    public IFormFile? File { get; set; }
}

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

public sealed record OpportunityCreateDto(
    int ClientId,
    string Name,
    string? Description,
    DateTime? DateCreation,
    int? LeadSourceId,
    int? CompanySizeId,
    string? ContactCompany,
    string? TaxId,
    int? SegmentId,
    string? CountryCode,
    string? StateCode,
    string? City,
    string? Seller,
    int? OfficeId,
    int? FunnelStageId,
    int? StatusId,
    string? ReasonLost,
    DateTime? DateActualStage,
    DateTime? DateNextAction,
    string? Notes,
    int? RelationshipLevelId,
    int? UrgencyLevelId,
    int? TechnicalFitId,
    int? BudgetLevelId,
    int? ServiceTypeId,
    string? CurrencyCode,
    DateTime? ForecastDate,
    decimal? EstimatedValue);

public sealed record OpportunityUpdateDto(
    int? ClientId,
    string? Name,
    string? Description,
    DateTime? DateCreation,
    int? LeadSourceId,
    int? CompanySizeId,
    string? ContactCompany,
    string? TaxId,
    int? SegmentId,
    string? CountryCode,
    string? StateCode,
    string? City,
    string? Seller,
    int? OfficeId,
    int? FunnelStageId,
    int? StatusId,
    string? ReasonLost,
    DateTime? DateActualStage,
    DateTime? DateNextAction,
    string? Notes,
    int? RelationshipLevelId,
    int? UrgencyLevelId,
    int? TechnicalFitId,
    int? BudgetLevelId,
    int? ServiceTypeId,
    string? CurrencyCode,
    DateTime? ForecastDate,
    decimal? EstimatedValue,
    bool? Active);

public sealed record OpportunityQueryDto(
    int? ClientId,
    int? StatusId,
    string? Name,
    bool? Active,
    int? Page,
    int? PageSize);

public sealed record OpportunityDto(
    int Id,
    int ClientId,
    string ClientName,
    string Name,
    string? Description,
    DateTime? DateCreation,
    int? Week,
    int? LeadSourceId,
    string? LeadSourceName,
    int? CompanySizeId,
    string? CompanySizeName,
    string? ContactCompany,
    string? TaxId,
    int? SegmentId,
    string? SegmentName,
    string? CountryCode,
    string? CountryName,
    string? StateCode,
    string? StateName,
    string? City,
    string? Seller,
    int? OfficeId,
    string? OfficeName,
    int? FunnelStageId,
    string? FunnelStageName,
    int? StatusId,
    string? StatusName,
    string? ReasonLost,
    DateTime? DateActualStage,
    int? DaysOnStage,
    DateTime? DateNextAction,
    string? Notes,
    int? RelationshipLevelId,
    string? RelationshipLevelName,
    int? UrgencyLevelId,
    string? UrgencyLevelName,
    int? TechnicalFitId,
    string? TechnicalFitName,
    int? BudgetLevelId,
    string? BudgetLevelName,
    decimal? ProbabilityPercent,
    int? ServiceTypeId,
    string? ServiceTypeName,
    string? CurrencyCode,
    string? CurrencyName,
    DateTime? ForecastDate,
    decimal? EstimatedValue,
    bool Active,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ProposalCreateDto(
    int ClientId,
    int? OpportunityId,
    string Title,
    string? ObjectiveHtml,
    decimal ProjectHours,
    decimal GlobalMarginPercent,
    string? Status);

public sealed record ProposalUpdateDto(
    int? ClientId,
    int? OpportunityId,
    string? Title,
    string? ObjectiveHtml,
    decimal? ProjectHours,
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
    decimal ProjectHours,
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
    int ProposalEmployeeId,
    int ProposalId,
    int EmployeeId,
    string EmployeeName,
    decimal CostSnapshot,
    decimal MarginPercentApplied,
    decimal SellPriceSnapshot,
    decimal HourlyValueSnapshot,
    bool Active,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CommercialDashboardOpportunityDto(
    int OpportunityId,
    string ClientName,
    string Project,
    string Stage,
    decimal? Value,
    string? CurrencyCode,
    string Status);

public sealed record CommercialDashboardDto(
    int ActiveOpportunities,
    int OpenProposals,
    int ActiveClients,
    decimal TotalPipelineValue,
    decimal? AverageProbabilityPercent,
    IReadOnlyList<CommercialDashboardOpportunityDto> RecentOpportunities);
