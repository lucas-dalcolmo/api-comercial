using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Entities;
using Api.Comercial.Models.Responses;

namespace Api.Comercial.Services;

public interface IOpportunityService
{
    Task<OperationResult<OpportunityDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<OperationResult<PagedResult<OpportunityDto>>> GetAllAsync(OpportunityQueryDto query, CancellationToken cancellationToken);
    Task<OperationResult<OpportunityDto>> CreateAsync(OpportunityCreateDto dto, CancellationToken cancellationToken);
    Task<OperationResult<OpportunityDto>> PatchAsync(int id, OpportunityUpdateDto dto, CancellationToken cancellationToken);
}

public sealed class OpportunityService : IOpportunityService
{
    private const int NameMaxLength = 200;
    private const int DescriptionMaxLength = 2000;

    private readonly ApeironDbContext _context;

    public OpportunityService(ApeironDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<OpportunityDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var item = await BuildProjection(_context.Opportunities.AsNoTracking().Where(o => o.Id == id && o.Active))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return OperationResult<OpportunityDto>.Fail("not_found", "Opportunity not found.");
        }

        return OperationResult<OpportunityDto>.Ok(item);
    }

    public async Task<OperationResult<PagedResult<OpportunityDto>>> GetAllAsync(OpportunityQueryDto query, CancellationToken cancellationToken)
    {
        var currentPage = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);
        var name = query.Name?.Trim();

        var dataQuery = _context.Opportunities.AsNoTracking().AsQueryable();

        if (query.ClientId.HasValue)
        {
            dataQuery = dataQuery.Where(o => o.ClientId == query.ClientId.Value);
        }

        if (query.StatusId.HasValue)
        {
            dataQuery = dataQuery.Where(o => o.StatusId == query.StatusId.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            dataQuery = dataQuery.Where(o => o.Name.Contains(name) || (o.ContactCompany != null && o.ContactCompany.Contains(name)));
        }

        if (query.Active.HasValue)
        {
            dataQuery = dataQuery.Where(o => o.Active == query.Active.Value);
        }
        else
        {
            dataQuery = dataQuery.Where(o => o.Active);
        }

        var totalCount = await dataQuery.CountAsync(cancellationToken);
        var items = await BuildProjection(dataQuery
                .OrderByDescending(o => o.UpdatedAt)
                .ThenByDescending(o => o.Id)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize))
            .ToListAsync(cancellationToken);

        return OperationResult<PagedResult<OpportunityDto>>.Ok(new PagedResult<OpportunityDto>(items, currentPage, pageSize, totalCount));
    }

    public async Task<OperationResult<OpportunityDto>> CreateAsync(OpportunityCreateDto dto, CancellationToken cancellationToken)
    {
        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<OpportunityDto>.Fail("validation", "Name is required.");
        }

        if (name.Length > NameMaxLength)
        {
            return OperationResult<OpportunityDto>.Fail("validation", $"Name must be at most {NameMaxLength} characters.");
        }

        var clientExists = await _context.Clients
            .AsNoTracking()
            .AnyAsync(c => c.Id == dto.ClientId && c.Active, cancellationToken);
        if (!clientExists)
        {
            return OperationResult<OpportunityDto>.Fail("not_found", "Client not found.");
        }

        var description = NormalizeNullableTrim(dto.Description);
        if (description is not null && description.Length > DescriptionMaxLength)
        {
            return OperationResult<OpportunityDto>.Fail("validation", $"Description must be at most {DescriptionMaxLength} characters.");
        }

        var referencesValidation = await ValidateReferencesAsync(
            dto.LeadSourceId,
            dto.CompanySizeId,
            dto.SegmentId,
            dto.OfficeId,
            dto.FunnelStageId,
            dto.StatusId,
            dto.RelationshipLevelId,
            dto.UrgencyLevelId,
            dto.TechnicalFitId,
            dto.BudgetLevelId,
            dto.ServiceTypeId,
            dto.CountryCode,
            dto.StateCode,
            dto.CurrencyCode,
            cancellationToken);
        if (!referencesValidation.Success)
        {
            return OperationResult<OpportunityDto>.Fail(referencesValidation.ErrorCode!, referencesValidation.ErrorMessage!);
        }

        var now = DateTime.UtcNow;
        var entity = new Opportunity
        {
            ClientId = dto.ClientId,
            Name = name,
            Description = description,
            DateCreation = dto.DateCreation?.Date,
            Week = ComputeWeek(dto.DateCreation),
            LeadSourceId = dto.LeadSourceId,
            CompanySizeId = dto.CompanySizeId,
            ContactCompany = NormalizeNullableTrim(dto.ContactCompany),
            TaxId = NormalizeNullableTrim(dto.TaxId),
            SegmentId = dto.SegmentId,
            CountryCode = NormalizeCode(dto.CountryCode),
            StateCode = NormalizeCode(dto.StateCode),
            City = NormalizeNullableTrim(dto.City),
            Seller = NormalizeNullableTrim(dto.Seller),
            OfficeId = dto.OfficeId,
            FunnelStageId = dto.FunnelStageId,
            StatusId = dto.StatusId,
            ReasonLost = NormalizeNullableTrim(dto.ReasonLost),
            DateActualStage = dto.DateActualStage?.Date,
            DaysOnStage = ComputeDaysOnStage(dto.DateActualStage),
            DateNextAction = dto.DateNextAction?.Date,
            Notes = NormalizeNullableTrim(dto.Notes),
            RelationshipLevelId = dto.RelationshipLevelId,
            UrgencyLevelId = dto.UrgencyLevelId,
            TechnicalFitId = dto.TechnicalFitId,
            BudgetLevelId = dto.BudgetLevelId,
            ServiceTypeId = dto.ServiceTypeId,
            CurrencyCode = NormalizeCode(dto.CurrencyCode),
            ForecastDate = dto.ForecastDate?.Date,
            EstimatedValue = dto.EstimatedValue,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        entity.ProbabilityPercent = await ComputeProbabilityPercentAsync(entity, cancellationToken);

        _context.Opportunities.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<OpportunityDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var dtoResult = await BuildProjection(_context.Opportunities.AsNoTracking().Where(o => o.Id == entity.Id))
            .FirstAsync(cancellationToken);

        return OperationResult<OpportunityDto>.Ok(dtoResult);
    }

    public async Task<OperationResult<OpportunityDto>> PatchAsync(int id, OpportunityUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (entity is null || !entity.Active)
        {
            return OperationResult<OpportunityDto>.Fail("not_found", "Opportunity not found.");
        }

        if (dto.ClientId is null
            && dto.Name is null
            && dto.Description is null
            && dto.DateCreation is null
            && dto.LeadSourceId is null
            && dto.CompanySizeId is null
            && dto.ContactCompany is null
            && dto.TaxId is null
            && dto.SegmentId is null
            && dto.CountryCode is null
            && dto.StateCode is null
            && dto.City is null
            && dto.Seller is null
            && dto.OfficeId is null
            && dto.FunnelStageId is null
            && dto.StatusId is null
            && dto.ReasonLost is null
            && dto.DateActualStage is null
            && dto.DateNextAction is null
            && dto.Notes is null
            && dto.RelationshipLevelId is null
            && dto.UrgencyLevelId is null
            && dto.TechnicalFitId is null
            && dto.BudgetLevelId is null
            && dto.ServiceTypeId is null
            && dto.CurrencyCode is null
            && dto.ForecastDate is null
            && dto.EstimatedValue is null
            && !dto.Active.HasValue)
        {
            return OperationResult<OpportunityDto>.Fail("validation", "At least one field must be provided.");
        }

        if (dto.ClientId.HasValue)
        {
            var clientExists = await _context.Clients
                .AsNoTracking()
                .AnyAsync(c => c.Id == dto.ClientId.Value && c.Active, cancellationToken);
            if (!clientExists)
            {
                return OperationResult<OpportunityDto>.Fail("not_found", "Client not found.");
            }

            entity.ClientId = dto.ClientId.Value;
        }

        if (dto.Name is not null)
        {
            var name = dto.Name.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return OperationResult<OpportunityDto>.Fail("validation", "Name is required.");
            }

            if (name.Length > NameMaxLength)
            {
                return OperationResult<OpportunityDto>.Fail("validation", $"Name must be at most {NameMaxLength} characters.");
            }

            entity.Name = name;
        }

        if (dto.Description is not null)
        {
            var description = dto.Description.Trim();
            if (description.Length > DescriptionMaxLength)
            {
                return OperationResult<OpportunityDto>.Fail("validation", $"Description must be at most {DescriptionMaxLength} characters.");
            }

            entity.Description = string.IsNullOrWhiteSpace(description) ? null : description;
        }

        var referencesValidation = await ValidateReferencesAsync(
            dto.LeadSourceId ?? entity.LeadSourceId,
            dto.CompanySizeId ?? entity.CompanySizeId,
            dto.SegmentId ?? entity.SegmentId,
            dto.OfficeId ?? entity.OfficeId,
            dto.FunnelStageId ?? entity.FunnelStageId,
            dto.StatusId ?? entity.StatusId,
            dto.RelationshipLevelId ?? entity.RelationshipLevelId,
            dto.UrgencyLevelId ?? entity.UrgencyLevelId,
            dto.TechnicalFitId ?? entity.TechnicalFitId,
            dto.BudgetLevelId ?? entity.BudgetLevelId,
            dto.ServiceTypeId ?? entity.ServiceTypeId,
            dto.CountryCode ?? entity.CountryCode,
            dto.StateCode ?? entity.StateCode,
            dto.CurrencyCode ?? entity.CurrencyCode,
            cancellationToken);
        if (!referencesValidation.Success)
        {
            return OperationResult<OpportunityDto>.Fail(referencesValidation.ErrorCode!, referencesValidation.ErrorMessage!);
        }

        if (dto.DateCreation.HasValue)
        {
            entity.DateCreation = dto.DateCreation.Value.Date;
            entity.Week = ComputeWeek(dto.DateCreation);
        }

        if (dto.LeadSourceId.HasValue) entity.LeadSourceId = dto.LeadSourceId.Value;
        if (dto.CompanySizeId.HasValue) entity.CompanySizeId = dto.CompanySizeId.Value;
        if (dto.ContactCompany is not null) entity.ContactCompany = NormalizeNullableTrim(dto.ContactCompany);
        if (dto.TaxId is not null) entity.TaxId = NormalizeNullableTrim(dto.TaxId);
        if (dto.SegmentId.HasValue) entity.SegmentId = dto.SegmentId.Value;
        if (dto.CountryCode is not null) entity.CountryCode = NormalizeCode(dto.CountryCode);
        if (dto.StateCode is not null) entity.StateCode = NormalizeCode(dto.StateCode);
        if (dto.City is not null) entity.City = NormalizeNullableTrim(dto.City);
        if (dto.Seller is not null) entity.Seller = NormalizeNullableTrim(dto.Seller);
        if (dto.OfficeId.HasValue) entity.OfficeId = dto.OfficeId.Value;
        if (dto.FunnelStageId.HasValue) entity.FunnelStageId = dto.FunnelStageId.Value;
        if (dto.StatusId.HasValue) entity.StatusId = dto.StatusId.Value;
        if (dto.ReasonLost is not null) entity.ReasonLost = NormalizeNullableTrim(dto.ReasonLost);

        if (dto.DateActualStage.HasValue)
        {
            entity.DateActualStage = dto.DateActualStage.Value.Date;
            entity.DaysOnStage = ComputeDaysOnStage(dto.DateActualStage);
        }

        if (dto.DateNextAction.HasValue) entity.DateNextAction = dto.DateNextAction.Value.Date;
        if (dto.Notes is not null) entity.Notes = NormalizeNullableTrim(dto.Notes);
        if (dto.RelationshipLevelId.HasValue) entity.RelationshipLevelId = dto.RelationshipLevelId.Value;
        if (dto.UrgencyLevelId.HasValue) entity.UrgencyLevelId = dto.UrgencyLevelId.Value;
        if (dto.TechnicalFitId.HasValue) entity.TechnicalFitId = dto.TechnicalFitId.Value;
        if (dto.BudgetLevelId.HasValue) entity.BudgetLevelId = dto.BudgetLevelId.Value;
        if (dto.ServiceTypeId.HasValue) entity.ServiceTypeId = dto.ServiceTypeId.Value;
        if (dto.CurrencyCode is not null) entity.CurrencyCode = NormalizeCode(dto.CurrencyCode);
        if (dto.ForecastDate.HasValue) entity.ForecastDate = dto.ForecastDate.Value.Date;
        if (dto.EstimatedValue.HasValue) entity.EstimatedValue = dto.EstimatedValue.Value;

        if (dto.Active.HasValue)
        {
            entity.Active = dto.Active.Value;
        }

        entity.ProbabilityPercent = await ComputeProbabilityPercentAsync(entity, cancellationToken);
        entity.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<OpportunityDto>.Fail("db_error", $"Database error: {ex.GetBaseException().Message}");
        }

        var dtoResult = await BuildProjection(_context.Opportunities.AsNoTracking().Where(o => o.Id == entity.Id))
            .FirstAsync(cancellationToken);

        return OperationResult<OpportunityDto>.Ok(dtoResult);
    }

    private IQueryable<OpportunityDto> BuildProjection(IQueryable<Opportunity> query)
        => query.Select(o => new OpportunityDto(
            o.Id,
            o.ClientId,
            o.Client != null ? o.Client.Name : string.Empty,
            o.Name,
            o.Description,
            o.DateCreation,
            o.Week,
            o.LeadSourceId,
            o.LeadSource != null ? o.LeadSource.Name : null,
            o.CompanySizeId,
            o.CompanySize != null ? o.CompanySize.Name : null,
            o.ContactCompany,
            o.TaxId,
            o.SegmentId,
            o.Segment != null ? o.Segment.Name : null,
            o.CountryCode,
            o.Country != null ? o.Country.Name : null,
            o.StateCode,
            o.State != null ? o.State.Name : null,
            o.City,
            o.Seller,
            o.OfficeId,
            o.Office != null ? o.Office.Name : null,
            o.FunnelStageId,
            o.FunnelStage != null ? o.FunnelStage.Name : null,
            o.StatusId,
            o.Status != null ? o.Status.Name : null,
            o.ReasonLost,
            o.DateActualStage,
            o.DaysOnStage,
            o.DateNextAction,
            o.Notes,
            o.RelationshipLevelId,
            o.RelationshipLevel != null ? o.RelationshipLevel.Name : null,
            o.UrgencyLevelId,
            o.UrgencyLevel != null ? o.UrgencyLevel.Name : null,
            o.TechnicalFitId,
            o.TechnicalFit != null ? o.TechnicalFit.Name : null,
            o.BudgetLevelId,
            o.BudgetLevel != null ? o.BudgetLevel.Name : null,
            o.ProbabilityPercent,
            o.ServiceTypeId,
            o.ServiceType != null ? o.ServiceType.Name : null,
            o.CurrencyCode,
            o.Currency != null ? o.Currency.Name : null,
            o.ForecastDate,
            o.EstimatedValue,
            o.Active,
            o.CreatedAt,
            o.UpdatedAt));

    private async Task<OperationResult<bool>> ValidateReferencesAsync(
        int? leadSourceId,
        int? companySizeId,
        int? segmentId,
        int? officeId,
        int? funnelStageId,
        int? statusId,
        int? relationshipLevelId,
        int? urgencyLevelId,
        int? technicalFitId,
        int? budgetLevelId,
        int? serviceTypeId,
        string? countryCode,
        string? stateCode,
        string? currencyCode,
        CancellationToken cancellationToken)
    {
        if (leadSourceId.HasValue && !await _context.LeadSources.AsNoTracking().AnyAsync(x => x.Id == leadSourceId.Value && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Lead source not found.");
        if (companySizeId.HasValue && !await _context.CompanySizes.AsNoTracking().AnyAsync(x => x.Id == companySizeId.Value && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Company size not found.");
        if (segmentId.HasValue && !await _context.Segments.AsNoTracking().AnyAsync(x => x.Id == segmentId.Value && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Segment not found.");
        if (officeId.HasValue && !await _context.Offices.AsNoTracking().AnyAsync(x => x.Id == officeId.Value && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Office not found.");
        if (funnelStageId.HasValue && !await _context.FunnelStages.AsNoTracking().AnyAsync(x => x.Id == funnelStageId.Value && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Funnel stage not found.");
        if (statusId.HasValue && !await _context.OpportunityStatuses.AsNoTracking().AnyAsync(x => x.Id == statusId.Value && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Opportunity status not found.");
        if (relationshipLevelId.HasValue && !await _context.RelationshipLevels.AsNoTracking().AnyAsync(x => x.Id == relationshipLevelId.Value && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Relationship level not found.");
        if (urgencyLevelId.HasValue && !await _context.UrgencyLevels.AsNoTracking().AnyAsync(x => x.Id == urgencyLevelId.Value && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Urgency level not found.");
        if (technicalFitId.HasValue && !await _context.TechnicalFits.AsNoTracking().AnyAsync(x => x.Id == technicalFitId.Value && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Technical fit not found.");
        if (budgetLevelId.HasValue && !await _context.BudgetLevels.AsNoTracking().AnyAsync(x => x.Id == budgetLevelId.Value && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Budget level not found.");
        if (serviceTypeId.HasValue && !await _context.ServiceTypes.AsNoTracking().AnyAsync(x => x.Id == serviceTypeId.Value && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Service type not found.");
        if (!string.IsNullOrWhiteSpace(countryCode) && !await _context.Countries.AsNoTracking().AnyAsync(x => x.Code == countryCode && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Country not found.");
        if (!string.IsNullOrWhiteSpace(stateCode) && !await _context.States.AsNoTracking().AnyAsync(x => x.Code == stateCode && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "State not found.");
        if (!string.IsNullOrWhiteSpace(currencyCode) && !await _context.Currencies.AsNoTracking().AnyAsync(x => x.Code == currencyCode && x.Active, cancellationToken))
            return OperationResult<bool>.Fail("not_found", "Currency not found.");

        return OperationResult<bool>.Ok(true);
    }

    private async Task<decimal?> ComputeProbabilityPercentAsync(Opportunity opportunity, CancellationToken cancellationToken)
    {
        var statusName = await ResolveLookupNameAsync(_context.OpportunityStatuses, opportunity.StatusId, cancellationToken);
        var funnelStageName = await ResolveLookupNameAsync(_context.FunnelStages, opportunity.FunnelStageId, cancellationToken);
        var relationshipName = await ResolveLookupNameAsync(_context.RelationshipLevels, opportunity.RelationshipLevelId, cancellationToken);
        var urgencyName = await ResolveLookupNameAsync(_context.UrgencyLevels, opportunity.UrgencyLevelId, cancellationToken);
        var technicalFitName = await ResolveLookupNameAsync(_context.TechnicalFits, opportunity.TechnicalFitId, cancellationToken);
        var budgetName = await ResolveLookupNameAsync(_context.BudgetLevels, opportunity.BudgetLevelId, cancellationToken);

        if (string.Equals(statusName, "Closed - Lost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(statusName, "Lost", StringComparison.OrdinalIgnoreCase))
        {
            return 0m;
        }

        if (string.Equals(funnelStageName, "Deal", StringComparison.OrdinalIgnoreCase))
        {
            return 100m;
        }

        if (string.Equals(technicalFitName, "Out of scope", StringComparison.OrdinalIgnoreCase))
        {
            return 0m;
        }

        if (string.IsNullOrWhiteSpace(funnelStageName)
            || string.IsNullOrWhiteSpace(relationshipName)
            || string.IsNullOrWhiteSpace(urgencyName)
            || string.IsNullOrWhiteSpace(technicalFitName)
            || string.IsNullOrWhiteSpace(budgetName))
        {
            return null;
        }

        var categories = await _context.ScoreCategoryWeights
            .AsNoTracking()
            .Where(c => c.Active)
            .ToDictionaryAsync(c => c.CategoryName, c => c.CategoryWeight, StringComparer.OrdinalIgnoreCase, cancellationToken);

        if (!TryGetCategoryWeight(categories, "Funnel_Stage", out var funnelCategoryWeight)
            || !TryGetCategoryWeight(categories, "RelationShip_With_Client", out var relationshipCategoryWeight)
            || !TryGetCategoryWeight(categories, "Urgency", out var urgencyCategoryWeight)
            || !TryGetCategoryWeight(categories, "Technical_Fit", out var technicalCategoryWeight)
            || !TryGetCategoryWeight(categories, "Budget", out var budgetCategoryWeight))
        {
            return null;
        }

        var values = await _context.ScoreValueWeights
            .AsNoTracking()
            .Where(v => v.Active)
            .ToListAsync(cancellationToken);

        var funnelValue = TryGetValueWeight(values, "Funnel_Stage", funnelStageName);
        var relationshipValue = TryGetValueWeight(values, "RelationShip_With_Client", relationshipName);
        var urgencyValue = TryGetValueWeight(values, "Urgency", urgencyName);
        var technicalValue = TryGetValueWeight(values, "Technical_Fit", technicalFitName);
        var budgetValue = TryGetValueWeight(values, "Budget", budgetName);

        if (funnelValue is null || relationshipValue is null || urgencyValue is null || technicalValue is null || budgetValue is null)
        {
            return null;
        }

        var result = (funnelValue.Value * funnelCategoryWeight)
            + (relationshipValue.Value * relationshipCategoryWeight)
            + (urgencyValue.Value * urgencyCategoryWeight)
            + (technicalValue.Value * technicalCategoryWeight)
            + (budgetValue.Value * budgetCategoryWeight);

        return decimal.Round(result, 4, MidpointRounding.AwayFromZero);
    }

    private static bool TryGetCategoryWeight(IReadOnlyDictionary<string, decimal> categories, string name, out decimal value)
    {
        if (categories.TryGetValue(name, out value))
        {
            return true;
        }

        value = 0m;
        return false;
    }

    private static decimal? TryGetValueWeight(IReadOnlyList<ScoreValueWeight> values, string categoryName, string valueName)
    {
        var entry = values.FirstOrDefault(v =>
            string.Equals(v.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(v.ValueName, valueName, StringComparison.OrdinalIgnoreCase));

        return entry?.ValueWeight;
    }

    private static async Task<string?> ResolveLookupNameAsync<TEntity>(IQueryable<TEntity> source, int? id, CancellationToken cancellationToken)
        where TEntity : IntLookupEntity
    {
        if (!id.HasValue)
        {
            return null;
        }

        return await source
            .AsNoTracking()
            .Where(x => x.Id == id.Value)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static int? ComputeWeek(DateTime? dateCreation)
    {
        if (!dateCreation.HasValue)
        {
            return null;
        }

        var calendar = CultureInfo.InvariantCulture.Calendar;
        var date = dateCreation.Value.Date;
        return calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    private static int? ComputeDaysOnStage(DateTime? dateActualStage)
    {
        if (!dateActualStage.HasValue)
        {
            return null;
        }

        var delta = DateTime.UtcNow.Date - dateActualStage.Value.Date;
        return delta.Days < 0 ? 0 : delta.Days;
    }

    private static string? NormalizeNullableTrim(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeCode(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

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
