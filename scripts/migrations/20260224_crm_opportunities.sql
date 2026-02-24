-- Migration: CRM Opportunities + proposal opportunity FK/index

IF OBJECT_ID('crm.Opportunity','U') IS NULL
BEGIN
    CREATE TABLE crm.Opportunity (
        OpportunityId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClientId INT NOT NULL,
        OpportunityName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(2000) NULL,
        StatusId INT NULL,
        Active BIT NOT NULL CONSTRAINT DF_Opportunity_Active DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Opportunity_CreatedAt DEFAULT(SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Opportunity_UpdatedAt DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_Opportunity_Client FOREIGN KEY (ClientId) REFERENCES crm.Client(ClientId),
        CONSTRAINT FK_Opportunity_Status FOREIGN KEY (StatusId) REFERENCES crm.dm_OpportunityStatus(StatusId)
    );
END
GO

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'OpportunityName')
BEGIN
    ALTER TABLE crm.Opportunity ADD OpportunityName NVARCHAR(200) NULL;
END
GO

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'OpportunityName')
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'Name')
BEGIN
    EXEC('UPDATE crm.Opportunity SET OpportunityName = CAST([Name] AS NVARCHAR(200)) WHERE OpportunityName IS NULL');
END
GO

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'OpportunityName')
BEGIN
    UPDATE crm.Opportunity
    SET OpportunityName = CONCAT('Opportunity ', OpportunityId)
    WHERE OpportunityName IS NULL OR LTRIM(RTRIM(OpportunityName)) = '';
END
GO

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'OpportunityName')
BEGIN
    ALTER TABLE crm.Opportunity ALTER COLUMN OpportunityName NVARCHAR(200) NOT NULL;
END
GO

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'Description')
    ALTER TABLE crm.Opportunity ADD Description NVARCHAR(2000) NULL;
GO

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'StatusId')
    ALTER TABLE crm.Opportunity ADD StatusId INT NULL;
GO

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'CreatedAt')
    ALTER TABLE crm.Opportunity ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Opportunity_CreatedAt DEFAULT(SYSUTCDATETIME());
GO

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Opportunity') AND name = 'UpdatedAt')
    ALTER TABLE crm.Opportunity ADD UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Opportunity_UpdatedAt DEFAULT(SYSUTCDATETIME());
GO

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_Client')
BEGIN
    ALTER TABLE crm.Opportunity
        ADD CONSTRAINT FK_Opportunity_Client FOREIGN KEY (ClientId) REFERENCES crm.Client(ClientId);
END
GO

IF OBJECT_ID('crm.Opportunity','U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Opportunity_Status')
BEGIN
    ALTER TABLE crm.Opportunity
        ADD CONSTRAINT FK_Opportunity_Status FOREIGN KEY (StatusId) REFERENCES crm.dm_OpportunityStatus(StatusId);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Opportunity_Client_Active'
      AND object_id = OBJECT_ID('crm.Opportunity')
)
    CREATE INDEX IX_Opportunity_Client_Active ON crm.Opportunity(ClientId, Active);
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Opportunity_Status_Active'
      AND object_id = OBJECT_ID('crm.Opportunity')
)
    CREATE INDEX IX_Opportunity_Status_Active ON crm.Opportunity(StatusId, Active);
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Proposal_Opportunity_Active'
      AND object_id = OBJECT_ID('crm.Proposal')
)
    CREATE INDEX IX_Proposal_Opportunity_Active ON crm.Proposal(OpportunityId, Active);
GO

IF OBJECT_ID('crm.Proposal','U') IS NOT NULL
   AND OBJECT_ID('crm.Opportunity','U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Proposal_Opportunity')
BEGIN
    ALTER TABLE crm.Proposal
        ADD CONSTRAINT FK_Proposal_Opportunity FOREIGN KEY (OpportunityId) REFERENCES crm.Opportunity(OpportunityId);
END
GO
