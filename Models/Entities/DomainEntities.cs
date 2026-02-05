namespace Api.Comercial.Models.Entities;

public abstract class IntLookupEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

public abstract class CodeNameEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

public sealed class BloodType : IntLookupEntity { }
public sealed class BudgetLevel : IntLookupEntity { }
public sealed class CompanySize : IntLookupEntity { }
public sealed class Department : IntLookupEntity { }
public sealed class EducationLevel : IntLookupEntity { }
public sealed class EmploymentType : IntLookupEntity { }
public sealed class FunnelStage : IntLookupEntity { }
public sealed class Gender : IntLookupEntity { }
public sealed class InteractionType : IntLookupEntity { }
public sealed class LeadSource : IntLookupEntity { }
public sealed class MaritalStatus : IntLookupEntity { }
public sealed class Office : IntLookupEntity { }
public sealed class OpportunityStatus : IntLookupEntity { }
public sealed class Region : IntLookupEntity { }
public sealed class RelationshipLevel : IntLookupEntity { }
public sealed class Role : IntLookupEntity { }
public sealed class Segment : IntLookupEntity { }
public sealed class ServiceType : IntLookupEntity { }
public sealed class TechnicalFit : IntLookupEntity { }
public sealed class UrgencyLevel : IntLookupEntity { }

public sealed class Country : CodeNameEntity { }
public sealed class Currency : CodeNameEntity { }

public sealed class State
{
    public string Code { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}
