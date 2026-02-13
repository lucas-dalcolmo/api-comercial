namespace Api.Comercial.Models.Entities;

public sealed class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? LogoUrl { get; set; }
    public bool Active { get; set; }

    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
}

public sealed class Proposal
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? OpportunityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ObjectiveHtml { get; set; } = string.Empty;
    public decimal GlobalMarginPercent { get; set; }
    public string Status { get; set; } = ProposalStatus.Draft;
    public decimal TotalCost { get; set; }
    public decimal TotalSellPrice { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Client? Client { get; set; }
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
