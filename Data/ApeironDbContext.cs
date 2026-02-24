using Microsoft.EntityFrameworkCore;
using Api.Comercial.Models.Entities;

namespace Api.Comercial.Data;

public partial class ApeironDbContext : DbContext
{
    public ApeironDbContext(DbContextOptions<ApeironDbContext> options)
        : base(options)
    {
    }

    public DbSet<BloodType> BloodTypes => Set<BloodType>();
    public DbSet<BudgetLevel> BudgetLevels => Set<BudgetLevel>();
    public DbSet<CompanySize> CompanySizes => Set<CompanySize>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<EducationLevel> EducationLevels => Set<EducationLevel>();
    public DbSet<EmploymentType> EmploymentTypes => Set<EmploymentType>();
    public DbSet<FunnelStage> FunnelStages => Set<FunnelStage>();
    public DbSet<Gender> Genders => Set<Gender>();
    public DbSet<InteractionType> InteractionTypes => Set<InteractionType>();
    public DbSet<LeadSource> LeadSources => Set<LeadSource>();
    public DbSet<MaritalStatus> MaritalStatuses => Set<MaritalStatus>();
    public DbSet<Office> Offices => Set<Office>();
    public DbSet<OpportunityStatus> OpportunityStatuses => Set<OpportunityStatus>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<RelationshipLevel> RelationshipLevels => Set<RelationshipLevel>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Segment> Segments => Set<Segment>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<TechnicalFit> TechnicalFits => Set<TechnicalFit>();
    public DbSet<UrgencyLevel> UrgencyLevels => Set<UrgencyLevel>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<BenefitType> BenefitTypes => Set<BenefitType>();
    public DbSet<Nationality> Nationalities => Set<Nationality>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<State> States => Set<State>();
    public DbSet<ScoreCategoryWeight> ScoreCategoryWeights => Set<ScoreCategoryWeight>();
    public DbSet<ScoreValueWeight> ScoreValueWeights => Set<ScoreValueWeight>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeContract> EmployeeContracts => Set<EmployeeContract>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<EmployeeBenefit> EmployeeBenefits => Set<EmployeeBenefit>();
    public DbSet<EmployeeContractBenefit> EmployeeContractBenefits => Set<EmployeeContractBenefit>();
    public DbSet<BenefitFormulaVariable> BenefitFormulaVariables => Set<BenefitFormulaVariable>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ProposalEmployee> ProposalEmployees => Set<ProposalEmployee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureIntLookup<BloodType>(modelBuilder, "dm_BloodType", "BloodTypeId", "BloodTypeName", 5, uniqueNameIndex: true);
        ConfigureIntLookup<BudgetLevel>(modelBuilder, "dm_BudgetLevel", "BudgetLevelId", "BudgetLevelName", 60, uniqueNameIndex: true);
        ConfigureIntLookup<CompanySize>(modelBuilder, "dm_CompanySize", "CompanySizeId", "CompanySizeName", 30, uniqueNameIndex: true);
        ConfigureIntLookup<Department>(modelBuilder, "dm_Department", "DepartmentId", "DepartmentName", 80, uniqueNameIndex: true);
        ConfigureIntLookup<EducationLevel>(modelBuilder, "dm_EducationLevel", "EducationLevelId", "EducationLevelName", 80, uniqueNameIndex: true);
        ConfigureIntLookup<EmploymentType>(modelBuilder, "dm_EmploymentType", "EmploymentTypeId", "EmploymentTypeName", 20, uniqueNameIndex: true);
        ConfigureIntLookup<FunnelStage>(modelBuilder, "dm_FunnelStage", "FunnelStageId", "FunnelStageName", 60, uniqueNameIndex: true);
        ConfigureIntLookup<Gender>(modelBuilder, "dm_Gender", "GenderId", "GenderName", 30, uniqueNameIndex: true);
        ConfigureIntLookup<InteractionType>(modelBuilder, "dm_InteractionType", "InteractionTypeId", "InteractionTypeName", 50, uniqueNameIndex: true);
        ConfigureIntLookup<LeadSource>(modelBuilder, "dm_LeadSource", "LeadSourceId", "LeadSourceName", 50, uniqueNameIndex: true);
        ConfigureIntLookup<MaritalStatus>(modelBuilder, "dm_MaritalStatus", "MaritalStatusId", "MaritalStatusName", 40, uniqueNameIndex: true);
        ConfigureIntLookup<Office>(modelBuilder, "dm_Office", "OfficeId", "OfficeName", 80, uniqueNameIndex: true);
        ConfigureIntLookup<OpportunityStatus>(modelBuilder, "dm_OpportunityStatus", "StatusId", "StatusName", 50, uniqueNameIndex: true);
        ConfigureIntLookup<Region>(modelBuilder, "dm_Region", "RegionId", "RegionName", 80, uniqueNameIndex: true);
        ConfigureIntLookup<RelationshipLevel>(modelBuilder, "dm_RelationshipLevel", "RelationshipLevelId", "RelationshipLevelName", 60, uniqueNameIndex: true);
        ConfigureIntLookup<Role>(modelBuilder, "dm_Role", "RoleId", "RoleName", 120, uniqueNameIndex: true);
        ConfigureIntLookup<Segment>(modelBuilder, "dm_Segment", "SegmentId", "SegmentName", 100, uniqueNameIndex: true);
        ConfigureIntLookup<ServiceType>(modelBuilder, "dm_ServiceType", "ServiceTypeId", "ServiceTypeName", 80, uniqueNameIndex: true);
        ConfigureIntLookup<TechnicalFit>(modelBuilder, "dm_TechnicalFit", "TechnicalFitId", "TechnicalFitName", 60, uniqueNameIndex: true);
        ConfigureIntLookup<UrgencyLevel>(modelBuilder, "dm_UrgencyLevel", "UrgencyLevelId", "UrgencyLevelName", 40, uniqueNameIndex: true);
        ConfigureIntLookup<DocumentType>(modelBuilder, "dm_DocumentType", "DocumentTypeId", "DocumentTypeName", 80, uniqueNameIndex: true);
        ConfigureIntLookup<BenefitType>(modelBuilder, "dm_BenefitType", "BenefitTypeId", "BenefitTypeName", 80, uniqueNameIndex: true);
        ConfigureIntLookup<Nationality>(modelBuilder, "dm_Nationality", "NationalityId", "NationalityName", 80, uniqueNameIndex: true);

        ConfigureCodeName<Country>(modelBuilder, "dm_Country", "CountryCode", 2, "CountryName", 80);
        ConfigureCodeName<Currency>(modelBuilder, "dm_Currency", "CurrencyCode", 3, "CurrencyName", 50);

        modelBuilder.Entity<State>(entity =>
        {
            entity.ToTable("dm_State", "crm");
            entity.HasKey(e => e.Code).HasName("PK__dm_State__D515E98BB4ACAD76");
            entity.Property(e => e.Code).HasColumnName("StateCode").HasMaxLength(2).IsFixedLength();
            entity.Property(e => e.CountryCode).HasColumnName("CountryCode").HasMaxLength(2).IsFixedLength().IsRequired();
            entity.Property(e => e.Name).HasColumnName("StateName").HasMaxLength(80).IsRequired();
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
            entity.HasOne<Country>()
                .WithMany()
                .HasForeignKey(e => e.CountryCode)
                .HasConstraintName("FK_State_Country");
        });

        modelBuilder.Entity<ScoreCategoryWeight>(entity =>
        {
            entity.ToTable("ScoreCategoryWeight", "crm");
            entity.HasKey(e => e.CategoryName);
            entity.Property(e => e.CategoryName).HasColumnName("CategoryName").HasMaxLength(60).IsRequired();
            entity.Property(e => e.CategoryWeight).HasColumnName("CategoryWeight").HasColumnType("decimal(9,4)").IsRequired();
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
        });

        modelBuilder.Entity<ScoreValueWeight>(entity =>
        {
            entity.ToTable("ScoreValueWeight", "crm");
            entity.HasKey(e => new { e.CategoryName, e.ValueName });
            entity.Property(e => e.CategoryName).HasColumnName("CategoryName").HasMaxLength(60).IsRequired();
            entity.Property(e => e.ValueName).HasColumnName("ValueName").HasMaxLength(100).IsRequired();
            entity.Property(e => e.ValueWeight).HasColumnName("ValueWeight").HasColumnType("decimal(9,4)").IsRequired();
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
            entity.HasOne<ScoreCategoryWeight>()
                .WithMany()
                .HasForeignKey(e => e.CategoryName)
                .HasConstraintName("FK_ScoreValueWeight_Category");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee", "hr");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("EmployeeId").ValueGeneratedOnAdd();
            entity.Property(e => e.FullName).HasColumnName("FullName").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Cpf).HasColumnName("CPF").HasMaxLength(14);
            entity.Property(e => e.GenderId).HasColumnName("GenderId");
            entity.Property(e => e.BirthDate).HasColumnName("BirthDate").HasColumnType("date");
            entity.Property(e => e.Nationality).HasColumnName("Nationality").HasMaxLength(80);
            entity.Property(e => e.PlaceOfBirth).HasColumnName("PlaceOfBirth").HasMaxLength(120);
            entity.Property(e => e.MaritalStatusId).HasColumnName("MaritalStatusId");
            entity.Property(e => e.ChildrenCount).HasColumnName("ChildrenCount");
            entity.Property(e => e.Phone).HasColumnName("Phone").HasMaxLength(30);
            entity.Property(e => e.PersonalEmail).HasColumnName("PersonalEmail").HasMaxLength(255);
            entity.Property(e => e.CorporateEmail).HasColumnName("CorporateEmail").HasMaxLength(255);
            entity.Property(e => e.Address).HasColumnName("Address").HasMaxLength(300);
            entity.Property(e => e.EducationLevelId).HasColumnName("EducationLevelId");
            entity.Property(e => e.BloodTypeId).HasColumnName("BloodTypeId");
            entity.Property(e => e.HireDate).HasColumnName("HireDate").HasColumnType("date");
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
            entity.HasIndex(e => e.FullName).HasDatabaseName("IX_Employee_FullName");
        });

        modelBuilder.Entity<EmployeeContract>(entity =>
        {
            entity.ToTable("EmployeeContract", "hr");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ContractId").ValueGeneratedOnAdd();
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeId");
            entity.Property(e => e.EmploymentTypeId).HasColumnName("EmploymentTypeId");
            entity.Property(e => e.Cnpj).HasColumnName("CNPJ").HasMaxLength(18);
            entity.Property(e => e.RoleId).HasColumnName("RoleId");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentId");
            entity.Property(e => e.RegionId).HasColumnName("RegionId");
            entity.Property(e => e.OfficeId).HasColumnName("OfficeId");
            entity.Property(e => e.BaseSalaryUsd).HasColumnName("BaseSalaryUsd").HasColumnType("decimal(18,2)");
            entity.Property(e => e.StartDate).HasColumnName("StartDate").HasColumnType("date");
            entity.Property(e => e.EndDate).HasColumnName("EndDate").HasColumnType("date");
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.Contracts)
                .HasForeignKey(e => e.EmployeeId)
                .HasConstraintName("FK_EmployeeContract_Employee");
        });

        modelBuilder.Entity<EmployeeContractBenefit>(entity =>
        {
            entity.ToTable("EmployeeContractBenefit", "hr");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ContractBenefitId").ValueGeneratedOnAdd();
            entity.Property(e => e.ContractId).HasColumnName("ContractId").IsRequired();
            entity.Property(e => e.BenefitTypeId).HasColumnName("BenefitTypeId");
            entity.Property(e => e.Value).HasColumnName("BenefitValue").HasColumnType("decimal(18,2)");
            entity.Property(e => e.IsFormula).HasColumnName("IsFormula").IsRequired();
            entity.Property(e => e.Formula).HasColumnName("Formula").HasMaxLength(500);
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
            entity.HasOne(e => e.Contract)
                .WithMany(e => e.Benefits)
                .HasForeignKey(e => e.ContractId)
                .HasConstraintName("FK_EmployeeContractBenefit_Contract");
        });

        modelBuilder.Entity<EmployeeDocument>(entity =>
        {
            entity.ToTable("EmployeeDocument", "hr");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("DocumentId").ValueGeneratedOnAdd();
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeId").IsRequired();
            entity.Property(e => e.DocumentTypeId).HasColumnName("DocumentTypeId");
            entity.Property(e => e.DocumentNumber).HasColumnName("DocumentNumber").HasMaxLength(120);
            entity.Property(e => e.CountryCode).HasColumnName("CountryCode").HasMaxLength(2).IsFixedLength();
            entity.Property(e => e.IssueDate).HasColumnName("IssueDate").HasColumnType("date");
            entity.Property(e => e.ExpiryDate).HasColumnName("ExpiryDate").HasColumnType("date");
            entity.Property(e => e.Notes).HasColumnName("Notes").HasMaxLength(500);
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.Documents)
                .HasForeignKey(e => e.EmployeeId)
                .HasConstraintName("FK_EmployeeDocument_Employee");
        });

        modelBuilder.Entity<EmployeeBenefit>(entity =>
        {
            entity.ToTable("EmployeeBenefit", "hr");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("BenefitId").ValueGeneratedOnAdd();
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeId").IsRequired();
            entity.Property(e => e.BenefitTypeId).HasColumnName("BenefitTypeId");
            entity.Property(e => e.StartDate).HasColumnName("StartDate").HasColumnType("date");
            entity.Property(e => e.EndDate).HasColumnName("EndDate").HasColumnType("date");
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.Benefits)
                .HasForeignKey(e => e.EmployeeId)
                .HasConstraintName("FK_EmployeeBenefit_Employee");
        });

        modelBuilder.Entity<BenefitFormulaVariable>(entity =>
        {
            entity.ToTable("BenefitFormulaVariable", "hr");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("BenefitFormulaVariableId").ValueGeneratedOnAdd();
            entity.Property(e => e.VariableKey).HasColumnName("VariableKey").HasMaxLength(150).IsRequired();
            entity.Property(e => e.SourceScope).HasColumnName("SourceScope").HasMaxLength(40).IsRequired();
            entity.Property(e => e.SourceSchema).HasColumnName("SourceSchema").HasMaxLength(30).IsRequired();
            entity.Property(e => e.SourceTable).HasColumnName("SourceTable").HasMaxLength(128).IsRequired();
            entity.Property(e => e.SourceColumn).HasColumnName("SourceColumn").HasMaxLength(128).IsRequired();
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
            entity.HasIndex(e => e.VariableKey).IsUnique();
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Client", "crm");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ClientId").ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnName("ClientName").HasMaxLength(200).IsRequired();
            entity.Property(e => e.LegalName).HasColumnName("LegalName").HasMaxLength(300);
            entity.Property(e => e.LogoUrl).HasColumnName("LogoUrl").HasMaxLength(500);
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
        });

        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.ToTable("Opportunity", "crm");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("OpportunityId").ValueGeneratedOnAdd();
            entity.Property(e => e.ClientId).HasColumnName("ClientId").IsRequired();
            entity.Property(e => e.Name).HasColumnName("OpportunityName").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasColumnName("Description").HasMaxLength(2000);
            entity.Property(e => e.DateCreation).HasColumnName("DateCreation").HasColumnType("date");
            entity.Property(e => e.Week).HasColumnName("Week");
            entity.Property(e => e.LeadSourceId).HasColumnName("LeadSourceId");
            entity.Property(e => e.CompanySizeId).HasColumnName("CompanySizeId");
            entity.Property(e => e.ContactCompany).HasColumnName("ContactCompany").HasMaxLength(200);
            entity.Property(e => e.TaxId).HasColumnName("TaxId").HasMaxLength(30);
            entity.Property(e => e.SegmentId).HasColumnName("SegmentId");
            entity.Property(e => e.CountryCode).HasColumnName("CountryCode").HasMaxLength(2).IsFixedLength();
            entity.Property(e => e.StateCode).HasColumnName("StateCode").HasMaxLength(2).IsFixedLength();
            entity.Property(e => e.City).HasColumnName("City").HasMaxLength(120);
            entity.Property(e => e.Seller).HasColumnName("Seller").HasMaxLength(120);
            entity.Property(e => e.OfficeId).HasColumnName("OfficeId");
            entity.Property(e => e.FunnelStageId).HasColumnName("FunnelStageId");
            entity.Property(e => e.StatusId).HasColumnName("StatusId");
            entity.Property(e => e.ReasonLost).HasColumnName("ReasonLost").HasMaxLength(500);
            entity.Property(e => e.DateActualStage).HasColumnName("DateActualStage").HasColumnType("date");
            entity.Property(e => e.DaysOnStage).HasColumnName("DaysOnStage");
            entity.Property(e => e.DateNextAction).HasColumnName("DateNextAction").HasColumnType("date");
            entity.Property(e => e.Notes).HasColumnName("Notes").HasMaxLength(1000);
            entity.Property(e => e.RelationshipLevelId).HasColumnName("RelationshipLevelId");
            entity.Property(e => e.UrgencyLevelId).HasColumnName("UrgencyLevelId");
            entity.Property(e => e.TechnicalFitId).HasColumnName("TechnicalFitId");
            entity.Property(e => e.BudgetLevelId).HasColumnName("BudgetLevelId");
            entity.Property(e => e.ProbabilityPercent).HasColumnName("ProbabilityPercent").HasColumnType("decimal(9,4)");
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeId");
            entity.Property(e => e.CurrencyCode).HasColumnName("CurrencyCode").HasMaxLength(3).IsFixedLength();
            entity.Property(e => e.ForecastDate).HasColumnName("ForecastDate").HasColumnType("date");
            entity.Property(e => e.EstimatedValue).HasColumnName("EstimatedValue").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("datetime2").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("datetime2").IsRequired();

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Opportunities)
                .HasForeignKey(e => e.ClientId)
                .HasConstraintName("FK_Opportunity_Client");

            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .HasConstraintName("FK_Opportunity_Status");

            entity.HasOne(e => e.LeadSource)
                .WithMany()
                .HasForeignKey(e => e.LeadSourceId)
                .HasConstraintName("FK_Opportunity_LeadSource");

            entity.HasOne(e => e.CompanySize)
                .WithMany()
                .HasForeignKey(e => e.CompanySizeId)
                .HasConstraintName("FK_Opportunity_CompanySize");

            entity.HasOne(e => e.Segment)
                .WithMany()
                .HasForeignKey(e => e.SegmentId)
                .HasConstraintName("FK_Opportunity_Segment");

            entity.HasOne(e => e.Country)
                .WithMany()
                .HasForeignKey(e => e.CountryCode)
                .HasConstraintName("FK_Opportunity_Country");

            entity.HasOne(e => e.State)
                .WithMany()
                .HasForeignKey(e => e.StateCode)
                .HasConstraintName("FK_Opportunity_State");

            entity.HasOne(e => e.Office)
                .WithMany()
                .HasForeignKey(e => e.OfficeId)
                .HasConstraintName("FK_Opportunity_Office");

            entity.HasOne(e => e.FunnelStage)
                .WithMany()
                .HasForeignKey(e => e.FunnelStageId)
                .HasConstraintName("FK_Opportunity_FunnelStage");

            entity.HasOne(e => e.RelationshipLevel)
                .WithMany()
                .HasForeignKey(e => e.RelationshipLevelId)
                .HasConstraintName("FK_Opportunity_RelationshipLevel");

            entity.HasOne(e => e.UrgencyLevel)
                .WithMany()
                .HasForeignKey(e => e.UrgencyLevelId)
                .HasConstraintName("FK_Opportunity_UrgencyLevel");

            entity.HasOne(e => e.TechnicalFit)
                .WithMany()
                .HasForeignKey(e => e.TechnicalFitId)
                .HasConstraintName("FK_Opportunity_TechnicalFit");

            entity.HasOne(e => e.BudgetLevel)
                .WithMany()
                .HasForeignKey(e => e.BudgetLevelId)
                .HasConstraintName("FK_Opportunity_BudgetLevel");

            entity.HasOne(e => e.ServiceType)
                .WithMany()
                .HasForeignKey(e => e.ServiceTypeId)
                .HasConstraintName("FK_Opportunity_ServiceType");

            entity.HasOne(e => e.Currency)
                .WithMany()
                .HasForeignKey(e => e.CurrencyCode)
                .HasConstraintName("FK_Opportunity_Currency");

            entity.HasIndex(e => new { e.ClientId, e.Active }).HasDatabaseName("IX_Opportunity_Client_Active");
            entity.HasIndex(e => new { e.StatusId, e.Active }).HasDatabaseName("IX_Opportunity_Status_Active");
        });

        modelBuilder.Entity<Proposal>(entity =>
        {
            entity.ToTable("Proposal", "crm");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ProposalId").ValueGeneratedOnAdd();
            entity.Property(e => e.ClientId).HasColumnName("ClientId").IsRequired();
            entity.Property(e => e.OpportunityId).HasColumnName("OpportunityId");
            entity.Property(e => e.Title).HasColumnName("Title").HasMaxLength(200).IsRequired();
            entity.Property(e => e.ObjectiveHtml).HasColumnName("ObjectiveHtml").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.ProjectHours).HasColumnName("ProjectHours").HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.GlobalMarginPercent).HasColumnName("GlobalMarginPercent").HasColumnType("decimal(9,4)").IsRequired();
            entity.Property(e => e.Status).HasColumnName("Status").HasMaxLength(40).IsRequired();
            entity.Property(e => e.TotalCost).HasColumnName("TotalCost").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.TotalSellPrice).HasColumnName("TotalSellPrice").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("datetime2").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("datetime2").IsRequired();

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Proposals)
                .HasForeignKey(e => e.ClientId)
                .HasConstraintName("FK_Proposal_Client");

            entity.HasOne(e => e.Opportunity)
                .WithMany(o => o.Proposals)
                .HasForeignKey(e => e.OpportunityId)
                .HasConstraintName("FK_Proposal_Opportunity");

            entity.HasIndex(e => new { e.ClientId, e.Active });
            entity.HasIndex(e => new { e.Status, e.Active });
            entity.HasIndex(e => new { e.OpportunityId, e.Active }).HasDatabaseName("IX_Proposal_Opportunity_Active");
        });

        modelBuilder.Entity<ProposalEmployee>(entity =>
        {
            entity.ToTable("ProposalEmployee", "crm");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ProposalEmployeeId").ValueGeneratedOnAdd();
            entity.Property(e => e.ProposalId).HasColumnName("ProposalId").IsRequired();
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeId").IsRequired();
            entity.Property(e => e.CostSnapshot).HasColumnName("CostSnapshot").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.MarginPercentApplied).HasColumnName("MarginPercentApplied").HasColumnType("decimal(9,4)").IsRequired();
            entity.Property(e => e.SellPriceSnapshot).HasColumnName("SellPriceSnapshot").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.HourlyValueSnapshot).HasColumnName("HourlyValueSnapshot").HasColumnType("decimal(18,4)").IsRequired();
            entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("datetime2").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("datetime2").IsRequired();

            entity.HasOne(e => e.Proposal)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.ProposalId)
                .HasConstraintName("FK_ProposalEmployee_Proposal");

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .HasConstraintName("FK_ProposalEmployee_Employee");

            entity.HasIndex(e => new { e.ProposalId, e.EmployeeId, e.Active })
                .IsUnique()
                .HasFilter("[Active] = 1");
            entity.HasIndex(e => new { e.ProposalId, e.Active }).HasDatabaseName("IX_ProposalEmployee_Proposal_Active");
            entity.HasIndex(e => e.EmployeeId).HasDatabaseName("IX_ProposalEmployee_Employee");
        });
    }

    private static void ConfigureIntLookup<TEntity>(
        ModelBuilder modelBuilder,
        string table,
        string idColumn,
        string nameColumn,
        int nameMaxLength,
        bool uniqueNameIndex)
        where TEntity : IntLookupEntity
    {
        var entity = modelBuilder.Entity<TEntity>();
        entity.ToTable(table, "crm");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName(idColumn).ValueGeneratedOnAdd();
        entity.Property(e => e.Name).HasColumnName(nameColumn).HasMaxLength(nameMaxLength).IsRequired();
        entity.Property(e => e.Active).HasColumnName("Active").IsRequired();

        if (uniqueNameIndex)
        {
            entity.HasIndex(e => e.Name).IsUnique();
        }
    }

    private static void ConfigureCodeName<TEntity>(
        ModelBuilder modelBuilder,
        string table,
        string codeColumn,
        int codeLength,
        string nameColumn,
        int nameLength)
        where TEntity : CodeNameEntity
    {
        var entity = modelBuilder.Entity<TEntity>();
        entity.ToTable(table, "crm");
        entity.HasKey(e => e.Code);
        entity.Property(e => e.Code).HasColumnName(codeColumn).HasMaxLength(codeLength).IsFixedLength();
        entity.Property(e => e.Name).HasColumnName(nameColumn).HasMaxLength(nameLength).IsRequired();
        entity.Property(e => e.Active).HasColumnName("Active").IsRequired();
    }
}
