namespace Api.Comercial.Models.Entities;

public sealed class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? LogoUrl { get; set; }
    public bool Active { get; set; }

    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
}

public sealed class Opportunity
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? Week { get; set; }
    public int? LeadSourceId { get; set; }
    public int? CompanySizeId { get; set; }
    public string? ContactCompany { get; set; }
    public string? TaxId { get; set; }
    public int? SegmentId { get; set; }
    public string? CountryCode { get; set; }
    public string? StateCode { get; set; }
    public string? City { get; set; }
    public string? Seller { get; set; }
    public int? OfficeId { get; set; }
    public int? FunnelStageId { get; set; }
    public int? StatusId { get; set; }
    public string? ReasonLost { get; set; }
    public DateTime? DateActualStage { get; set; }
    public int? DaysOnStage { get; set; }
    public DateTime? DateNextAction { get; set; }
    public string? Notes { get; set; }
    public int? RelationshipLevelId { get; set; }
    public int? UrgencyLevelId { get; set; }
    public int? TechnicalFitId { get; set; }
    public int? BudgetLevelId { get; set; }
    public decimal? ProbabilityPercent { get; set; }
    public int? ServiceTypeId { get; set; }
    public string? CurrencyCode { get; set; }
    public DateTime? ForecastDate { get; set; }
    public decimal? EstimatedValue { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Client? Client { get; set; }
    public LeadSource? LeadSource { get; set; }
    public CompanySize? CompanySize { get; set; }
    public Segment? Segment { get; set; }
    public Country? Country { get; set; }
    public State? State { get; set; }
    public Office? Office { get; set; }
    public FunnelStage? FunnelStage { get; set; }
    public OpportunityStatus? Status { get; set; }
    public RelationshipLevel? RelationshipLevel { get; set; }
    public UrgencyLevel? UrgencyLevel { get; set; }
    public TechnicalFit? TechnicalFit { get; set; }
    public BudgetLevel? BudgetLevel { get; set; }
    public ServiceType? ServiceType { get; set; }
    public Currency? Currency { get; set; }
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
}

public sealed class Proposal
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? OpportunityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ObjectiveHtml { get; set; } = string.Empty;
    public decimal ProjectHours { get; set; }
    public decimal GlobalMarginPercent { get; set; }
    public string Status { get; set; } = ProposalStatus.Draft;
    public decimal TotalCost { get; set; }
    public decimal TotalSellPrice { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Client? Client { get; set; }
    public Opportunity? Opportunity { get; set; }
    public ICollection<ProposalEmployee> Employees { get; set; } = new List<ProposalEmployee>();
}

public sealed class ProposalEmployee
{
    public int Id { get; set; }
    public int ProposalId { get; set; }
    public int EmployeeId { get; set; }
    public decimal CostSnapshot { get; set; }
    public decimal MarginPercentApplied { get; set; }
    public decimal SellPriceSnapshot { get; set; }
    public decimal HourlyValueSnapshot { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Proposal? Proposal { get; set; }
    public Employee? Employee { get; set; }
}

public static class ProposalStatus
{
    public const string Draft = "Draft";
    public const string Sent = "Sent";
    public const string ClosedWon = "ClosedWon";
    public const string ClosedLost = "ClosedLost";
    public const string Cancelled = "Cancelled";

    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Sent,
        ClosedWon,
        ClosedLost,
        Cancelled
    };

    public static bool IsClosed(string status)
    {
        return status.Equals(ClosedWon, StringComparison.OrdinalIgnoreCase)
            || status.Equals(ClosedLost, StringComparison.OrdinalIgnoreCase)
            || status.Equals(Cancelled, StringComparison.OrdinalIgnoreCase);
    }
}
