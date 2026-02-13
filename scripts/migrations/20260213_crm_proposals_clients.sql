-- Migration: CRM Clients + Proposals + ProposalEmployees
-- Date: 2026-02-13
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'crm')
    EXEC('CREATE SCHEMA crm');
GO

IF OBJECT_ID('crm.Client','U') IS NULL
BEGIN
    CREATE TABLE crm.Client (
        ClientId INT NOT NULL PRIMARY KEY,
        ClientName NVARCHAR(200) NOT NULL,
        LegalName NVARCHAR(300) NULL,
        LogoUrl NVARCHAR(500) NULL,
        Active BIT NOT NULL CONSTRAINT DF_Client_Active DEFAULT(1)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Client') AND name = 'LegalName'
)
    ALTER TABLE crm.Client ADD LegalName NVARCHAR(300) NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Client') AND name = 'LogoUrl'
)
    ALTER TABLE crm.Client ADD LogoUrl NVARCHAR(500) NULL;
GO

IF OBJECT_ID('crm.Proposal','U') IS NULL
BEGIN
    CREATE TABLE crm.Proposal (
        ProposalId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClientId INT NOT NULL,
        OpportunityId INT NULL,
        Title NVARCHAR(200) NOT NULL,
        ObjectiveHtml NVARCHAR(MAX) NOT NULL,
        ProjectHours DECIMAL(10,2) NOT NULL CONSTRAINT DF_Proposal_ProjectHours DEFAULT(220),
        GlobalMarginPercent DECIMAL(9,4) NOT NULL,
        Status NVARCHAR(40) NOT NULL,
        TotalCost DECIMAL(18,2) NOT NULL CONSTRAINT DF_Proposal_TotalCost DEFAULT(0),
        TotalSellPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Proposal_TotalSellPrice DEFAULT(0),
        Active BIT NOT NULL CONSTRAINT DF_Proposal_Active DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Proposal_CreatedAt DEFAULT(SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Proposal_UpdatedAt DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_Proposal_Client FOREIGN KEY (ClientId) REFERENCES crm.Client(ClientId)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Proposal') AND name = 'ProjectHours'
)
    ALTER TABLE crm.Proposal ADD ProjectHours DECIMAL(10,2) NOT NULL CONSTRAINT DF_Proposal_ProjectHours DEFAULT(220);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Proposal') AND name = 'TotalCost'
)
    ALTER TABLE crm.Proposal ADD TotalCost DECIMAL(18,2) NOT NULL CONSTRAINT DF_Proposal_TotalCost DEFAULT(0);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Proposal') AND name = 'TotalSellPrice'
)
    ALTER TABLE crm.Proposal ADD TotalSellPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Proposal_TotalSellPrice DEFAULT(0);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Proposal') AND name = 'CreatedAt'
)
    ALTER TABLE crm.Proposal ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Proposal_CreatedAt DEFAULT(SYSUTCDATETIME());
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.Proposal') AND name = 'UpdatedAt'
)
    ALTER TABLE crm.Proposal ADD UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Proposal_UpdatedAt DEFAULT(SYSUTCDATETIME());
GO

IF OBJECT_ID('crm.ProposalEmployee','U') IS NULL
BEGIN
    CREATE TABLE crm.ProposalEmployee (
        ProposalEmployeeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProposalId INT NOT NULL,
        EmployeeId INT NOT NULL,
        CostSnapshot DECIMAL(18,2) NOT NULL,
        MarginPercentApplied DECIMAL(9,4) NOT NULL,
        SellPriceSnapshot DECIMAL(18,2) NOT NULL,
        HourlyValueSnapshot DECIMAL(18,4) NOT NULL CONSTRAINT DF_ProposalEmployee_HourlyValueSnapshot DEFAULT(0),
        Active BIT NOT NULL CONSTRAINT DF_ProposalEmployee_Active DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProposalEmployee_CreatedAt DEFAULT(SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProposalEmployee_UpdatedAt DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_ProposalEmployee_Proposal FOREIGN KEY (ProposalId) REFERENCES crm.Proposal(ProposalId),
        CONSTRAINT FK_ProposalEmployee_Employee FOREIGN KEY (EmployeeId) REFERENCES hr.Employee(EmployeeId)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.ProposalEmployee') AND name = 'HourlyValueSnapshot'
)
    ALTER TABLE crm.ProposalEmployee ADD HourlyValueSnapshot DECIMAL(18,4) NOT NULL CONSTRAINT DF_ProposalEmployee_HourlyValueSnapshot DEFAULT(0);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.ProposalEmployee') AND name = 'CreatedAt'
)
    ALTER TABLE crm.ProposalEmployee ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProposalEmployee_CreatedAt DEFAULT(SYSUTCDATETIME());
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('crm.ProposalEmployee') AND name = 'UpdatedAt'
)
    ALTER TABLE crm.ProposalEmployee ADD UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProposalEmployee_UpdatedAt DEFAULT(SYSUTCDATETIME());
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_ProposalEmployee_Proposal_Employee_Active'
      AND object_id = OBJECT_ID('crm.ProposalEmployee')
)
    CREATE UNIQUE INDEX UX_ProposalEmployee_Proposal_Employee_Active
    ON crm.ProposalEmployee(ProposalId, EmployeeId, Active)
    WHERE Active = 1;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ProposalEmployee_Proposal_Active'
      AND object_id = OBJECT_ID('crm.ProposalEmployee')
)
    CREATE INDEX IX_ProposalEmployee_Proposal_Active ON crm.ProposalEmployee(ProposalId, Active);
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ProposalEmployee_Employee'
      AND object_id = OBJECT_ID('crm.ProposalEmployee')
)
    CREATE INDEX IX_ProposalEmployee_Employee ON crm.ProposalEmployee(EmployeeId);
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Employee_FullName'
      AND object_id = OBJECT_ID('hr.Employee')
)
    CREATE INDEX IX_Employee_FullName ON hr.Employee(FullName);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Proposal_ProjectHours_Positive')
    ALTER TABLE crm.Proposal WITH CHECK ADD CONSTRAINT CK_Proposal_ProjectHours_Positive CHECK (ProjectHours > 0);
GO

IF NOT EXISTS (SELECT 1 FROM crm.Client WHERE ClientName = 'Fortlev Demo Client')
BEGIN
    DECLARE @NextClientId INT = (SELECT ISNULL(MAX(ClientId), 0) + 1 FROM crm.Client);
    INSERT INTO crm.Client (ClientId, ClientName, LegalName, LogoUrl, Active)
    VALUES (@NextClientId, 'Fortlev Demo Client', 'Fortlev Demo Client LTDA', '/uploads/client-logos/demo-client.png', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM hr.Employee WHERE FullName = 'Alice Demo')
    INSERT INTO hr.Employee (FullName, CPF, Active) VALUES ('Alice Demo', '000.000.000-01', 1);
GO

IF NOT EXISTS (SELECT 1 FROM hr.Employee WHERE FullName = 'Bruno Demo')
    INSERT INTO hr.Employee (FullName, CPF, Active) VALUES ('Bruno Demo', '000.000.000-02', 1);
GO

DECLARE @AliceId INT = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Alice Demo' ORDER BY EmployeeId DESC);
DECLARE @BrunoId INT = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Bruno Demo' ORDER BY EmployeeId DESC);
DECLARE @ClientId INT = (SELECT TOP 1 ClientId FROM crm.Client WHERE ClientName = 'Fortlev Demo Client' ORDER BY ClientId DESC);

IF @AliceId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM hr.EmployeeContract WHERE EmployeeId = @AliceId AND Active = 1)
    INSERT INTO hr.EmployeeContract (EmployeeId, BaseSalaryUsd, StartDate, Active) VALUES (@AliceId, 3000.00, CAST(SYSUTCDATETIME() AS DATE), 1);

IF @BrunoId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM hr.EmployeeContract WHERE EmployeeId = @BrunoId AND Active = 1)
    INSERT INTO hr.EmployeeContract (EmployeeId, BaseSalaryUsd, StartDate, Active) VALUES (@BrunoId, 5000.00, CAST(SYSUTCDATETIME() AS DATE), 1);
GO

DECLARE @AliceContractId INT = (
    SELECT TOP 1 ContractId
    FROM hr.EmployeeContract
    WHERE EmployeeId = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Alice Demo' ORDER BY EmployeeId DESC)
      AND Active = 1
    ORDER BY StartDate DESC, ContractId DESC
);

IF @AliceContractId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM hr.EmployeeContractBenefit WHERE ContractId = @AliceContractId AND Active = 1)
    INSERT INTO hr.EmployeeContractBenefit (ContractId, BenefitTypeId, BenefitValue, IsFormula, Formula, Active)
    VALUES (@AliceContractId, NULL, 300.00, 0, NULL, 1);
GO

DECLARE @ProposalId INT = NULL;
DECLARE @ClientIdSeed INT = (SELECT TOP 1 ClientId FROM crm.Client WHERE ClientName = 'Fortlev Demo Client' ORDER BY ClientId DESC);

IF @ClientIdSeed IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM crm.Proposal WHERE ClientId = @ClientIdSeed AND Title = 'Proposta Comercial Demo')
BEGIN
    INSERT INTO crm.Proposal
    (
        ClientId,
        OpportunityId,
        Title,
        ObjectiveHtml,
        ProjectHours,
        GlobalMarginPercent,
        Status,
        TotalCost,
        TotalSellPrice,
        Active,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @ClientIdSeed,
        NULL,
        'Proposta Comercial Demo',
        '<p><strong>Objetivo:</strong> fornecer time dedicado com margem global de 20%.</p>',
        220.00,
        20.0000,
        'Draft',
        0,
        0,
        1,
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    );
END
GO

DECLARE @ProposalIdSeed INT = (SELECT TOP 1 ProposalId FROM crm.Proposal WHERE Title = 'Proposta Comercial Demo' ORDER BY ProposalId DESC);
DECLARE @AliceIdSeed INT = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Alice Demo' ORDER BY EmployeeId DESC);
DECLARE @BrunoIdSeed INT = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Bruno Demo' ORDER BY EmployeeId DESC);

DECLARE @AliceCost DECIMAL(18,2) = (
    SELECT TOP 1 ISNULL(c.BaseSalaryUsd, 0) + ISNULL((
        SELECT SUM(ISNULL(b.BenefitValue, 0))
        FROM hr.EmployeeContractBenefit b
        WHERE b.ContractId = c.ContractId AND b.Active = 1 AND b.IsFormula = 0
    ), 0)
    FROM hr.EmployeeContract c
    WHERE c.EmployeeId = @AliceIdSeed AND c.Active = 1
    ORDER BY c.StartDate DESC, c.ContractId DESC
);

DECLARE @BrunoCost DECIMAL(18,2) = (
    SELECT TOP 1 ISNULL(c.BaseSalaryUsd, 0) + ISNULL((
        SELECT SUM(ISNULL(b.BenefitValue, 0))
        FROM hr.EmployeeContractBenefit b
        WHERE b.ContractId = c.ContractId AND b.Active = 1 AND b.IsFormula = 0
    ), 0)
    FROM hr.EmployeeContract c
    WHERE c.EmployeeId = @BrunoIdSeed AND c.Active = 1
    ORDER BY c.StartDate DESC, c.ContractId DESC
);

IF @ProposalIdSeed IS NOT NULL AND @AliceIdSeed IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM crm.ProposalEmployee WHERE ProposalId = @ProposalIdSeed AND EmployeeId = @AliceIdSeed AND Active = 1)
BEGIN
    INSERT INTO crm.ProposalEmployee
    (
        ProposalId,
        EmployeeId,
        CostSnapshot,
        MarginPercentApplied,
        SellPriceSnapshot,
        HourlyValueSnapshot,
        Active,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @ProposalIdSeed,
        @AliceIdSeed,
        ISNULL(@AliceCost, 0),
        20.0000,
        ISNULL(@AliceCost, 0) * 1.20,
        (ISNULL(@AliceCost, 0) * 1.20) / 220.0,
        1,
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    );
END

IF @ProposalIdSeed IS NOT NULL AND @BrunoIdSeed IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM crm.ProposalEmployee WHERE ProposalId = @ProposalIdSeed AND EmployeeId = @BrunoIdSeed AND Active = 1)
BEGIN
    INSERT INTO crm.ProposalEmployee
    (
        ProposalId,
        EmployeeId,
        CostSnapshot,
        MarginPercentApplied,
        SellPriceSnapshot,
        HourlyValueSnapshot,
        Active,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @ProposalIdSeed,
        @BrunoIdSeed,
        ISNULL(@BrunoCost, 0),
        20.0000,
        ISNULL(@BrunoCost, 0) * 1.20,
        (ISNULL(@BrunoCost, 0) * 1.20) / 220.0,
        1,
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    );
END

IF @ProposalIdSeed IS NOT NULL
BEGIN
    UPDATE p
    SET
        p.TotalCost = ISNULL(t.TotalCost, 0),
        p.TotalSellPrice = ISNULL(t.TotalSellPrice, 0),
        p.UpdatedAt = SYSUTCDATETIME()
    FROM crm.Proposal p
    OUTER APPLY (
        SELECT
            SUM(pe.CostSnapshot) AS TotalCost,
            SUM(pe.SellPriceSnapshot) AS TotalSellPrice
        FROM crm.ProposalEmployee pe
        WHERE pe.ProposalId = p.ProposalId
          AND pe.Active = 1
    ) t
    WHERE p.ProposalId = @ProposalIdSeed;
END
GO

UPDATE pe
SET pe.HourlyValueSnapshot = ROUND(pe.SellPriceSnapshot / 220.0, 4)
FROM crm.ProposalEmployee pe
WHERE pe.Active = 1
  AND (pe.HourlyValueSnapshot IS NULL OR pe.HourlyValueSnapshot = 0);
GO
