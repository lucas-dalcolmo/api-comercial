using Microsoft.AspNetCore.Mvc;
using Api.Comercial.Models.Entities;
using Api.Comercial.Services;

namespace Api.Comercial.Controllers.Domain;

[Route("api/domains/blood-types")]
public sealed class BloodTypesController : IntLookupControllerBase<BloodType>
{
    public BloodTypesController(IIntLookupService<BloodType> service) : base(service) { }
}

[Route("api/domains/budget-levels")]
public sealed class BudgetLevelsController : IntLookupControllerBase<BudgetLevel>
{
    public BudgetLevelsController(IIntLookupService<BudgetLevel> service) : base(service) { }
}

[Route("api/domains/company-sizes")]
public sealed class CompanySizesController : IntLookupControllerBase<CompanySize>
{
    public CompanySizesController(IIntLookupService<CompanySize> service) : base(service) { }
}

[Route("api/domains/departments")]
public sealed class DepartmentsController : IntLookupControllerBase<Department>
{
    public DepartmentsController(IIntLookupService<Department> service) : base(service) { }
}

[Route("api/domains/education-levels")]
public sealed class EducationLevelsController : IntLookupControllerBase<EducationLevel>
{
    public EducationLevelsController(IIntLookupService<EducationLevel> service) : base(service) { }
}

[Route("api/domains/employment-types")]
public sealed class EmploymentTypesController : IntLookupControllerBase<EmploymentType>
{
    public EmploymentTypesController(IIntLookupService<EmploymentType> service) : base(service) { }
}

[Route("api/domains/funnel-stages")]
public sealed class FunnelStagesController : IntLookupControllerBase<FunnelStage>
{
    public FunnelStagesController(IIntLookupService<FunnelStage> service) : base(service) { }
}

[Route("api/domains/genders")]
public sealed class GendersController : IntLookupControllerBase<Gender>
{
    public GendersController(IIntLookupService<Gender> service) : base(service) { }
}

[Route("api/domains/interaction-types")]
public sealed class InteractionTypesController : IntLookupControllerBase<InteractionType>
{
    public InteractionTypesController(IIntLookupService<InteractionType> service) : base(service) { }
}

[Route("api/domains/lead-sources")]
public sealed class LeadSourcesController : IntLookupControllerBase<LeadSource>
{
    public LeadSourcesController(IIntLookupService<LeadSource> service) : base(service) { }
}

[Route("api/domains/marital-statuses")]
public sealed class MaritalStatusesController : IntLookupControllerBase<MaritalStatus>
{
    public MaritalStatusesController(IIntLookupService<MaritalStatus> service) : base(service) { }
}

[Route("api/domains/offices")]
public sealed class OfficesController : IntLookupControllerBase<Office>
{
    public OfficesController(IIntLookupService<Office> service) : base(service) { }
}

[Route("api/domains/opportunity-statuses")]
public sealed class OpportunityStatusesController : IntLookupControllerBase<OpportunityStatus>
{
    public OpportunityStatusesController(IIntLookupService<OpportunityStatus> service) : base(service) { }
}

[Route("api/domains/regions")]
public sealed class RegionsController : IntLookupControllerBase<Region>
{
    public RegionsController(IIntLookupService<Region> service) : base(service) { }
}

[Route("api/domains/relationship-levels")]
public sealed class RelationshipLevelsController : IntLookupControllerBase<RelationshipLevel>
{
    public RelationshipLevelsController(IIntLookupService<RelationshipLevel> service) : base(service) { }
}

[Route("api/domains/roles")]
public sealed class RolesController : IntLookupControllerBase<Role>
{
    public RolesController(IIntLookupService<Role> service) : base(service) { }
}

[Route("api/domains/segments")]
public sealed class SegmentsController : IntLookupControllerBase<Segment>
{
    public SegmentsController(IIntLookupService<Segment> service) : base(service) { }
}

[Route("api/domains/service-types")]
public sealed class ServiceTypesController : IntLookupControllerBase<ServiceType>
{
    public ServiceTypesController(IIntLookupService<ServiceType> service) : base(service) { }
}

[Route("api/domains/technical-fits")]
public sealed class TechnicalFitsController : IntLookupControllerBase<TechnicalFit>
{
    public TechnicalFitsController(IIntLookupService<TechnicalFit> service) : base(service) { }
}

[Route("api/domains/urgency-levels")]
public sealed class UrgencyLevelsController : IntLookupControllerBase<UrgencyLevel>
{
    public UrgencyLevelsController(IIntLookupService<UrgencyLevel> service) : base(service) { }
}

[Route("api/domains/document-types")]
public sealed class DocumentTypesController : IntLookupControllerBase<DocumentType>
{
    public DocumentTypesController(IIntLookupService<DocumentType> service) : base(service) { }
}

[Route("api/domains/benefit-types")]
public sealed class BenefitTypesController : IntLookupControllerBase<BenefitType>
{
    public BenefitTypesController(IIntLookupService<BenefitType> service) : base(service) { }
}

[Route("api/domains/nationalities")]
public sealed class NationalitiesController : IntLookupControllerBase<Nationality>
{
    public NationalitiesController(IIntLookupService<Nationality> service) : base(service) { }
}

[Route("api/domains/countries")]
public sealed class CountriesController : CodeNameControllerBase<Country>
{
    public CountriesController(ICodeNameService<Country> service) : base(service) { }
}

[Route("api/domains/currencies")]
public sealed class CurrenciesController : CodeNameControllerBase<Currency>
{
    public CurrenciesController(ICodeNameService<Currency> service) : base(service) { }
}
