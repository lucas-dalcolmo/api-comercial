-- Migration: Opportunity dashboard fields + domains + score weights

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'DateCreation')
    ALTER TABLE crm.Opportunity ADD DateCreation DATE NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'Week')
    ALTER TABLE crm.Opportunity ADD [Week] INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'LeadSourceId')
    ALTER TABLE crm.Opportunity ADD LeadSourceId INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'CompanySizeId')
    ALTER TABLE crm.Opportunity ADD CompanySizeId INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'ContactCompany')
    ALTER TABLE crm.Opportunity ADD ContactCompany NVARCHAR(200) NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'TaxId')
    ALTER TABLE crm.Opportunity ADD TaxId NVARCHAR(30) NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'SegmentId')
    ALTER TABLE crm.Opportunity ADD SegmentId INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'CountryCode')
    ALTER TABLE crm.Opportunity ADD CountryCode CHAR(2) NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'StateCode')
    ALTER TABLE crm.Opportunity ADD StateCode CHAR(2) NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'City')
    ALTER TABLE crm.Opportunity ADD City NVARCHAR(120) NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'Seller')
    ALTER TABLE crm.Opportunity ADD Seller NVARCHAR(120) NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'OfficeId')
    ALTER TABLE crm.Opportunity ADD OfficeId INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'FunnelStageId')
    ALTER TABLE crm.Opportunity ADD FunnelStageId INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'ReasonLost')
    ALTER TABLE crm.Opportunity ADD ReasonLost NVARCHAR(500) NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'DateActualStage')
    ALTER TABLE crm.Opportunity ADD DateActualStage DATE NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'DaysOnStage')
    ALTER TABLE crm.Opportunity ADD DaysOnStage INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'DateNextAction')
    ALTER TABLE crm.Opportunity ADD DateNextAction DATE NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'Notes')
    ALTER TABLE crm.Opportunity ADD Notes NVARCHAR(1000) NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'RelationshipLevelId')
    ALTER TABLE crm.Opportunity ADD RelationshipLevelId INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'UrgencyLevelId')
    ALTER TABLE crm.Opportunity ADD UrgencyLevelId INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'TechnicalFitId')
    ALTER TABLE crm.Opportunity ADD TechnicalFitId INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'BudgetLevelId')
    ALTER TABLE crm.Opportunity ADD BudgetLevelId INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'ProbabilityPercent')
    ALTER TABLE crm.Opportunity ADD ProbabilityPercent DECIMAL(9,4) NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'ServiceTypeId')
    ALTER TABLE crm.Opportunity ADD ServiceTypeId INT NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'CurrencyCode')
    ALTER TABLE crm.Opportunity ADD CurrencyCode CHAR(3) NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'ForecastDate')
    ALTER TABLE crm.Opportunity ADD ForecastDate NVARCHAR(20) NULL;
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'EstimatedValue')
    ALTER TABLE crm.Opportunity ADD EstimatedValue DECIMAL(18,2) NULL;
GO

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_LeadSource')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_LeadSource FOREIGN KEY (LeadSourceId) REFERENCES crm.dm_LeadSource(LeadSourceId);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_CompanySize')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_CompanySize FOREIGN KEY (CompanySizeId) REFERENCES crm.dm_CompanySize(CompanySizeId);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_Segment')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_Segment FOREIGN KEY (SegmentId) REFERENCES crm.dm_Segment(SegmentId);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_Country')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_Country FOREIGN KEY (CountryCode) REFERENCES crm.dm_Country(CountryCode);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_State')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_State FOREIGN KEY (StateCode) REFERENCES crm.dm_State(StateCode);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_Office')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_Office FOREIGN KEY (OfficeId) REFERENCES crm.dm_Office(OfficeId);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_FunnelStage')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_FunnelStage FOREIGN KEY (FunnelStageId) REFERENCES crm.dm_FunnelStage(FunnelStageId);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_RelationshipLevel')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_RelationshipLevel FOREIGN KEY (RelationshipLevelId) REFERENCES crm.dm_RelationshipLevel(RelationshipLevelId);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_UrgencyLevel')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_UrgencyLevel FOREIGN KEY (UrgencyLevelId) REFERENCES crm.dm_UrgencyLevel(UrgencyLevelId);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_TechnicalFit')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_TechnicalFit FOREIGN KEY (TechnicalFitId) REFERENCES crm.dm_TechnicalFit(TechnicalFitId);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_BudgetLevel')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_BudgetLevel FOREIGN KEY (BudgetLevelId) REFERENCES crm.dm_BudgetLevel(BudgetLevelId);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_ServiceType')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_ServiceType FOREIGN KEY (ServiceTypeId) REFERENCES crm.dm_ServiceType(ServiceTypeId);
GO
IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_Currency')
    ALTER TABLE crm.Opportunity ADD CONSTRAINT FK_Opportunity_Currency FOREIGN KEY (CurrencyCode) REFERENCES crm.dm_Currency(CurrencyCode);
GO

IF NOT EXISTS (SELECT 1 FROM crm.dm_LeadSource WHERE LeadSourceName = 'External Demand') INSERT INTO crm.dm_LeadSource (LeadSourceName, Active) VALUES ('External Demand',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_LeadSource WHERE LeadSourceName = 'Internal Referral') INSERT INTO crm.dm_LeadSource (LeadSourceName, Active) VALUES ('Internal Referral',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_LeadSource WHERE LeadSourceName = 'External Referral') INSERT INTO crm.dm_LeadSource (LeadSourceName, Active) VALUES ('External Referral',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_LeadSource WHERE LeadSourceName = 'Network Search') INSERT INTO crm.dm_LeadSource (LeadSourceName, Active) VALUES ('Network Search',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_LeadSource WHERE LeadSourceName = 'Partners') INSERT INTO crm.dm_LeadSource (LeadSourceName, Active) VALUES ('Partners',1);
GO

IF NOT EXISTS (SELECT 1 FROM crm.dm_FunnelStage WHERE FunnelStageName = 'Lead') INSERT INTO crm.dm_FunnelStage (FunnelStageName, Active) VALUES ('Lead',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_FunnelStage WHERE FunnelStageName = 'Qualification') INSERT INTO crm.dm_FunnelStage (FunnelStageName, Active) VALUES ('Qualification',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_FunnelStage WHERE FunnelStageName = 'Proposal') INSERT INTO crm.dm_FunnelStage (FunnelStageName, Active) VALUES ('Proposal',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_FunnelStage WHERE FunnelStageName = 'Negotiation') INSERT INTO crm.dm_FunnelStage (FunnelStageName, Active) VALUES ('Negotiation',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_FunnelStage WHERE FunnelStageName = 'Deal') INSERT INTO crm.dm_FunnelStage (FunnelStageName, Active) VALUES ('Deal',1);
GO

IF NOT EXISTS (SELECT 1 FROM crm.dm_CompanySize WHERE CompanySizeName = 'Small') INSERT INTO crm.dm_CompanySize (CompanySizeName, Active) VALUES ('Small',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_CompanySize WHERE CompanySizeName = 'Medium') INSERT INTO crm.dm_CompanySize (CompanySizeName, Active) VALUES ('Medium',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_CompanySize WHERE CompanySizeName = 'Big') INSERT INTO crm.dm_CompanySize (CompanySizeName, Active) VALUES ('Big',1);
GO

IF NOT EXISTS (SELECT 1 FROM crm.dm_OpportunityStatus WHERE StatusName = 'Open') INSERT INTO crm.dm_OpportunityStatus (StatusName, Active) VALUES ('Open',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_OpportunityStatus WHERE StatusName = 'Closed - Won') INSERT INTO crm.dm_OpportunityStatus (StatusName, Active) VALUES ('Closed - Won',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_OpportunityStatus WHERE StatusName = 'Closed - Lost') INSERT INTO crm.dm_OpportunityStatus (StatusName, Active) VALUES ('Closed - Lost',1);
GO

IF NOT EXISTS (SELECT 1 FROM crm.dm_RelationshipLevel WHERE RelationshipLevelName = 'New Client') INSERT INTO crm.dm_RelationshipLevel (RelationshipLevelName, Active) VALUES ('New Client',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_RelationshipLevel WHERE RelationshipLevelName = 'First Technical contact') INSERT INTO crm.dm_RelationshipLevel (RelationshipLevelName, Active) VALUES ('First Technical contact',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_RelationshipLevel WHERE RelationshipLevelName = 'Worked once') INSERT INTO crm.dm_RelationshipLevel (RelationshipLevelName, Active) VALUES ('Worked once',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_RelationshipLevel WHERE RelationshipLevelName = 'Repeat Customer') INSERT INTO crm.dm_RelationshipLevel (RelationshipLevelName, Active) VALUES ('Repeat Customer',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_RelationshipLevel WHERE RelationshipLevelName = 'Estrategic_Customer') INSERT INTO crm.dm_RelationshipLevel (RelationshipLevelName, Active) VALUES ('Estrategic_Customer',1);
GO

IF NOT EXISTS (SELECT 1 FROM crm.dm_UrgencyLevel WHERE UrgencyLevelName = 'No urgency') INSERT INTO crm.dm_UrgencyLevel (UrgencyLevelName, Active) VALUES ('No urgency',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_UrgencyLevel WHERE UrgencyLevelName = 'Low') INSERT INTO crm.dm_UrgencyLevel (UrgencyLevelName, Active) VALUES ('Low',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_UrgencyLevel WHERE UrgencyLevelName = 'Medium') INSERT INTO crm.dm_UrgencyLevel (UrgencyLevelName, Active) VALUES ('Medium',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_UrgencyLevel WHERE UrgencyLevelName = 'High') INSERT INTO crm.dm_UrgencyLevel (UrgencyLevelName, Active) VALUES ('High',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_UrgencyLevel WHERE UrgencyLevelName = 'Shutdown') INSERT INTO crm.dm_UrgencyLevel (UrgencyLevelName, Active) VALUES ('Shutdown',1);
GO

IF NOT EXISTS (SELECT 1 FROM crm.dm_TechnicalFit WHERE TechnicalFitName = 'Out of scope') INSERT INTO crm.dm_TechnicalFit (TechnicalFitName, Active) VALUES ('Out of scope',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_TechnicalFit WHERE TechnicalFitName = 'High risk') INSERT INTO crm.dm_TechnicalFit (TechnicalFitName, Active) VALUES ('High risk',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_TechnicalFit WHERE TechnicalFitName = 'Medium') INSERT INTO crm.dm_TechnicalFit (TechnicalFitName, Active) VALUES ('Medium',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_TechnicalFit WHERE TechnicalFitName = 'High') INSERT INTO crm.dm_TechnicalFit (TechnicalFitName, Active) VALUES ('High',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_TechnicalFit WHERE TechnicalFitName = 'Fully compliant') INSERT INTO crm.dm_TechnicalFit (TechnicalFitName, Active) VALUES ('Fully compliant',1);
GO

IF NOT EXISTS (SELECT 1 FROM crm.dm_BudgetLevel WHERE BudgetLevelName = 'No budget') INSERT INTO crm.dm_BudgetLevel (BudgetLevelName, Active) VALUES ('No budget',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_BudgetLevel WHERE BudgetLevelName = 'Under definition') INSERT INTO crm.dm_BudgetLevel (BudgetLevelName, Active) VALUES ('Under definition',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_BudgetLevel WHERE BudgetLevelName = 'Estimated budget') INSERT INTO crm.dm_BudgetLevel (BudgetLevelName, Active) VALUES ('Estimated budget',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_BudgetLevel WHERE BudgetLevelName = 'Approved budget') INSERT INTO crm.dm_BudgetLevel (BudgetLevelName, Active) VALUES ('Approved budget',1);
GO

IF NOT EXISTS (SELECT 1 FROM crm.dm_ServiceType WHERE ServiceTypeName = 'Cybersecurity') INSERT INTO crm.dm_ServiceType (ServiceTypeName, Active) VALUES ('Cybersecurity',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_ServiceType WHERE ServiceTypeName = 'Maintenance') INSERT INTO crm.dm_ServiceType (ServiceTypeName, Active) VALUES ('Maintenance',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_ServiceType WHERE ServiceTypeName = 'Functional Safety') INSERT INTO crm.dm_ServiceType (ServiceTypeName, Active) VALUES ('Functional Safety',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_ServiceType WHERE ServiceTypeName = 'Integration') INSERT INTO crm.dm_ServiceType (ServiceTypeName, Active) VALUES ('Integration',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_ServiceType WHERE ServiceTypeName = 'Digital Tranformation') INSERT INTO crm.dm_ServiceType (ServiceTypeName, Active) VALUES ('Digital Tranformation',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_ServiceType WHERE ServiceTypeName = 'Projects') INSERT INTO crm.dm_ServiceType (ServiceTypeName, Active) VALUES ('Projects',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_ServiceType WHERE ServiceTypeName = 'Man power Support') INSERT INTO crm.dm_ServiceType (ServiceTypeName, Active) VALUES ('Man power Support',1);
GO

IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Chemicals') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Chemicals',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Petrochemicals') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Petrochemicals',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Marine & Offshore') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Marine & Offshore',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Mining') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Mining',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Ornamental Rocks') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Ornamental Rocks',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Metals') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Metals',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Steel Makings') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Steel Makings',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Power & Utilities') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Power & Utilities',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Building & Infrastructure') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Building & Infrastructure',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Industrial Automation') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Industrial Automation',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Food & Beverages') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Food & Beverages',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'General Industry') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('General Industry',1);
IF NOT EXISTS (SELECT 1 FROM crm.dm_Segment WHERE SegmentName = 'Other') INSERT INTO crm.dm_Segment (SegmentName, Active) VALUES ('Other',1);
GO

MERGE crm.ScoreCategoryWeight AS target
USING (VALUES
    ('Funnel_Stage', 30.0000),
    ('RelationShip_With_Client', 25.0000),
    ('Urgency', 15.0000),
    ('Technical_Fit', 10.0000),
    ('Budget', 20.0000)
) AS src(CategoryName, CategoryWeight)
ON target.CategoryName = src.CategoryName
WHEN MATCHED THEN UPDATE SET target.CategoryWeight = src.CategoryWeight, target.Active = 1
WHEN NOT MATCHED THEN INSERT (CategoryName, CategoryWeight, Active) VALUES (src.CategoryName, src.CategoryWeight, 1);
GO

MERGE crm.ScoreValueWeight AS target
USING (VALUES
    ('Funnel_Stage','Lead',0.1000),
    ('Funnel_Stage','Qualification',0.3000),
    ('Funnel_Stage','Proposal',0.6000),
    ('Funnel_Stage','Negotiation',0.8000),
    ('Funnel_Stage','Deal',1.0000),
    ('RelationShip_With_Client','New Client',0.1000),
    ('RelationShip_With_Client','First Technical contact',0.3000),
    ('RelationShip_With_Client','Worked once',0.6000),
    ('RelationShip_With_Client','Repeat Customer',0.8000),
    ('RelationShip_With_Client','Estrategic_Customer',0.9000),
    ('Urgency','No urgency',0.1000),
    ('Urgency','Low',0.3000),
    ('Urgency','Medium',0.5000),
    ('Urgency','High',0.7000),
    ('Urgency','Shutdown',0.9000),
    ('Technical_Fit','Out of scope',0.0000),
    ('Technical_Fit','High risk',0.2500),
    ('Technical_Fit','Medium',0.5000),
    ('Technical_Fit','High',0.7500),
    ('Technical_Fit','Fully compliant',0.9000),
    ('Budget','No budget',0.2000),
    ('Budget','Under definition',0.5000),
    ('Budget','Estimated budget',0.7000),
    ('Budget','Approved budget',0.9000)
) AS src(CategoryName, ValueName, ValueWeight)
ON target.CategoryName = src.CategoryName AND target.ValueName = src.ValueName
WHEN MATCHED THEN UPDATE SET target.ValueWeight = src.ValueWeight, target.Active = 1
WHEN NOT MATCHED THEN INSERT (CategoryName, ValueName, ValueWeight, Active) VALUES (src.CategoryName, src.ValueName, src.ValueWeight, 1);
GO
