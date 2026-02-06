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
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<State> States => Set<State>();

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
