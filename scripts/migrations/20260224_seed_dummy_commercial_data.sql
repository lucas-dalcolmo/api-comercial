USE Apeiron_ONE;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @NextOpportunityId INT = (SELECT ISNULL(MAX(OpportunityId), 0) + 1 FROM crm.Opportunity);

-- ---------------------------------------------------------------------------
-- 1) Ensure dummy clients
-- ---------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM crm.Client WHERE ClientName = 'DUMMY - Atlas Energia')
BEGIN
    DECLARE @ClientId1 INT = (SELECT ISNULL(MAX(ClientId), 0) + 1 FROM crm.Client);
    INSERT INTO crm.Client (ClientId, ClientName, LegalName, LogoUrl, Active)
    VALUES (@ClientId1, 'DUMMY - Atlas Energia', 'Atlas Energia S.A.', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM crm.Client WHERE ClientName = 'DUMMY - Horizonte Mineração')
BEGIN
    DECLARE @ClientId2 INT = (SELECT ISNULL(MAX(ClientId), 0) + 1 FROM crm.Client);
    INSERT INTO crm.Client (ClientId, ClientName, LegalName, LogoUrl, Active)
    VALUES (@ClientId2, 'DUMMY - Horizonte Mineração', 'Horizonte Mineração Ltda', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM crm.Client WHERE ClientName = 'DUMMY - Solaris Offshore')
BEGIN
    DECLARE @ClientId3 INT = (SELECT ISNULL(MAX(ClientId), 0) + 1 FROM crm.Client);
    INSERT INTO crm.Client (ClientId, ClientName, LegalName, LogoUrl, Active)
    VALUES (@ClientId3, 'DUMMY - Solaris Offshore', 'Solaris Offshore Serviços', NULL, 1);
END;

DECLARE @ClientAtlas INT = (SELECT TOP 1 ClientId FROM crm.Client WHERE ClientName = 'DUMMY - Atlas Energia' ORDER BY ClientId DESC);
DECLARE @ClientHorizonte INT = (SELECT TOP 1 ClientId FROM crm.Client WHERE ClientName = 'DUMMY - Horizonte Mineração' ORDER BY ClientId DESC);
DECLARE @ClientSolaris INT = (SELECT TOP 1 ClientId FROM crm.Client WHERE ClientName = 'DUMMY - Solaris Offshore' ORDER BY ClientId DESC);

-- ---------------------------------------------------------------------------
-- 2) Lookup ids (fallback to first active when specific name is missing)
-- ---------------------------------------------------------------------------
DECLARE @StatusOpen INT = (
    SELECT TOP 1 StatusId
    FROM crm.dm_OpportunityStatus
    WHERE Active = 1 AND StatusName IN ('Open', 'New', 'Qualified')
    ORDER BY CASE WHEN StatusName = 'Open' THEN 0 WHEN StatusName = 'New' THEN 1 ELSE 2 END, StatusId
);
IF @StatusOpen IS NULL
    SET @StatusOpen = (SELECT TOP 1 StatusId FROM crm.dm_OpportunityStatus WHERE Active = 1 ORDER BY StatusId);

DECLARE @StatusWon INT = (
    SELECT TOP 1 StatusId
    FROM crm.dm_OpportunityStatus
    WHERE Active = 1 AND StatusName IN ('Closed - Won', 'Won')
    ORDER BY CASE WHEN StatusName = 'Closed - Won' THEN 0 ELSE 1 END, StatusId
);
IF @StatusWon IS NULL
    SET @StatusWon = @StatusOpen;

DECLARE @FunnelLead INT = (SELECT TOP 1 FunnelStageId FROM crm.dm_FunnelStage WHERE Active = 1 AND FunnelStageName = 'Lead' ORDER BY FunnelStageId);
DECLARE @FunnelProposal INT = (SELECT TOP 1 FunnelStageId FROM crm.dm_FunnelStage WHERE Active = 1 AND FunnelStageName = 'Proposal' ORDER BY FunnelStageId);
DECLARE @FunnelNegotiation INT = (SELECT TOP 1 FunnelStageId FROM crm.dm_FunnelStage WHERE Active = 1 AND FunnelStageName = 'Negotiation' ORDER BY FunnelStageId);
DECLARE @FunnelDeal INT = (SELECT TOP 1 FunnelStageId FROM crm.dm_FunnelStage WHERE Active = 1 AND FunnelStageName = 'Deal' ORDER BY FunnelStageId);
IF @FunnelLead IS NULL SET @FunnelLead = (SELECT TOP 1 FunnelStageId FROM crm.dm_FunnelStage WHERE Active = 1 ORDER BY FunnelStageId);
IF @FunnelProposal IS NULL SET @FunnelProposal = @FunnelLead;
IF @FunnelNegotiation IS NULL SET @FunnelNegotiation = @FunnelProposal;
IF @FunnelDeal IS NULL SET @FunnelDeal = @FunnelNegotiation;

DECLARE @RelationshipNew INT = (SELECT TOP 1 RelationshipLevelId FROM crm.dm_RelationshipLevel WHERE Active = 1 AND RelationshipLevelName = 'New Client' ORDER BY RelationshipLevelId);
DECLARE @RelationshipRepeat INT = (SELECT TOP 1 RelationshipLevelId FROM crm.dm_RelationshipLevel WHERE Active = 1 AND RelationshipLevelName = 'Repeat Customer' ORDER BY RelationshipLevelId);
IF @RelationshipNew IS NULL SET @RelationshipNew = (SELECT TOP 1 RelationshipLevelId FROM crm.dm_RelationshipLevel WHERE Active = 1 ORDER BY RelationshipLevelId);
IF @RelationshipRepeat IS NULL SET @RelationshipRepeat = @RelationshipNew;

DECLARE @UrgencyLow INT = (SELECT TOP 1 UrgencyLevelId FROM crm.dm_UrgencyLevel WHERE Active = 1 AND UrgencyLevelName = 'Low' ORDER BY UrgencyLevelId);
DECLARE @UrgencyHigh INT = (SELECT TOP 1 UrgencyLevelId FROM crm.dm_UrgencyLevel WHERE Active = 1 AND UrgencyLevelName IN ('High', 'Shutdown') ORDER BY UrgencyLevelId);
IF @UrgencyLow IS NULL SET @UrgencyLow = (SELECT TOP 1 UrgencyLevelId FROM crm.dm_UrgencyLevel WHERE Active = 1 ORDER BY UrgencyLevelId);
IF @UrgencyHigh IS NULL SET @UrgencyHigh = @UrgencyLow;

DECLARE @TechnicalMedium INT = (SELECT TOP 1 TechnicalFitId FROM crm.dm_TechnicalFit WHERE Active = 1 AND TechnicalFitName IN ('Medium', 'High', 'Fully compliant') ORDER BY TechnicalFitId);
DECLARE @TechnicalHigh INT = (SELECT TOP 1 TechnicalFitId FROM crm.dm_TechnicalFit WHERE Active = 1 AND TechnicalFitName IN ('High', 'Fully compliant') ORDER BY TechnicalFitId);
IF @TechnicalMedium IS NULL SET @TechnicalMedium = (SELECT TOP 1 TechnicalFitId FROM crm.dm_TechnicalFit WHERE Active = 1 ORDER BY TechnicalFitId);
IF @TechnicalHigh IS NULL SET @TechnicalHigh = @TechnicalMedium;

DECLARE @BudgetEstimated INT = (SELECT TOP 1 BudgetLevelId FROM crm.dm_BudgetLevel WHERE Active = 1 AND BudgetLevelName = 'Estimated budget' ORDER BY BudgetLevelId);
DECLARE @BudgetApproved INT = (SELECT TOP 1 BudgetLevelId FROM crm.dm_BudgetLevel WHERE Active = 1 AND BudgetLevelName = 'Approved budget' ORDER BY BudgetLevelId);
IF @BudgetEstimated IS NULL SET @BudgetEstimated = (SELECT TOP 1 BudgetLevelId FROM crm.dm_BudgetLevel WHERE Active = 1 ORDER BY BudgetLevelId);
IF @BudgetApproved IS NULL SET @BudgetApproved = @BudgetEstimated;

DECLARE @ServiceTypeId INT = (SELECT TOP 1 ServiceTypeId FROM crm.dm_ServiceType WHERE Active = 1 ORDER BY ServiceTypeId);
DECLARE @LeadSourceId INT = (SELECT TOP 1 LeadSourceId FROM crm.dm_LeadSource WHERE Active = 1 ORDER BY LeadSourceId);
DECLARE @CompanySizeId INT = (SELECT TOP 1 CompanySizeId FROM crm.dm_CompanySize WHERE Active = 1 ORDER BY CompanySizeId);
DECLARE @SegmentId INT = (SELECT TOP 1 SegmentId FROM crm.dm_Segment WHERE Active = 1 ORDER BY SegmentId);
DECLARE @OfficeId INT = (SELECT TOP 1 OfficeId FROM crm.dm_Office WHERE Active = 1 ORDER BY OfficeId);
DECLARE @CountryCode CHAR(2) = (SELECT TOP 1 CountryCode FROM crm.dm_Country WHERE Active = 1 ORDER BY CASE WHEN CountryCode = 'BR' THEN 0 ELSE 1 END, CountryCode);
DECLARE @StateCode CHAR(2) = (
    SELECT TOP 1 StateCode
    FROM crm.dm_State
    WHERE Active = 1 AND (@CountryCode IS NULL OR CountryCode = @CountryCode)
    ORDER BY StateCode
);
DECLARE @CurrencyCode CHAR(3) = (SELECT TOP 1 CurrencyCode FROM crm.dm_Currency WHERE Active = 1 ORDER BY CASE WHEN CurrencyCode = 'USD' THEN 0 WHEN CurrencyCode = 'BRL' THEN 1 ELSE 2 END, CurrencyCode);

-- ---------------------------------------------------------------------------
-- 3) Insert 5 dummy opportunities
-- ---------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM crm.Opportunity WHERE OpportunityName = 'DUMMY - Expansão Subestação Sul')
BEGIN
    INSERT INTO crm.Opportunity
    (
        OpportunityId,
        ClientId, ContactId, StatusId, ValueAmount, CurrencyCode, OpenDate, CloseDate,
        LeadSourceId, FunnelStageId, CompanySizeId, OfficeId, ReasonLost,
        StageChangedDate, NextActionDate, Notes, RelationshipLevelId, UrgencyLevelId,
        TechnicalFitId, BudgetLevelId, Probability, ServiceTypeId, ForecastDate, EstimatedValue,
        ForecastDealValue, Active, OpportunityName, Description, CreatedAt, UpdatedAt,
        DateCreation, Week, ContactCompany, TaxId, SegmentId, CountryCode, StateCode, City, Seller,
        DateActualStage, DaysOnStage, DateNextAction, ProbabilityPercent
    )
    VALUES
    (
        @NextOpportunityId,
        @ClientAtlas, NULL, @StatusOpen, 250000.00, @CurrencyCode, CAST(DATEADD(DAY, -21, @Now) AS DATE), NULL,
        @LeadSourceId, @FunnelProposal, @CompanySizeId, @OfficeId, NULL,
        CAST(DATEADD(DAY, -3, @Now) AS DATE), CAST(DATEADD(DAY, 5, @Now) AS DATE), N'Pipeline aquecido com engenharia',
        @RelationshipRepeat, @UrgencyHigh, @TechnicalHigh, @BudgetApproved, NULL, @ServiceTypeId, CAST(DATEADD(DAY, 20, @Now) AS DATE), 250000.00,
        237500.00, 1, N'DUMMY - Expansão Subestação Sul', N'Escopo para expansão elétrica com equipe dedicada.', @Now, @Now,
        CAST(DATEADD(DAY, -21, @Now) AS DATE), DATEPART(ISO_WEEK, DATEADD(DAY, -21, @Now)), N'Atlas Energia', N'12.345.678/0001-90',
        @SegmentId, @CountryCode, @StateCode, N'Vitória', N'Lucas Dalcolmo',
        CAST(DATEADD(DAY, -3, @Now) AS DATE), 3, CAST(DATEADD(DAY, 5, @Now) AS DATE), 78.0000
    );
    SET @NextOpportunityId = @NextOpportunityId + 1;
END;

IF NOT EXISTS (SELECT 1 FROM crm.Opportunity WHERE OpportunityName = 'DUMMY - Parada de Manutenção Mina Norte')
BEGIN
    INSERT INTO crm.Opportunity
    (
        OpportunityId,
        ClientId, ContactId, StatusId, ValueAmount, CurrencyCode, OpenDate, CloseDate,
        LeadSourceId, FunnelStageId, CompanySizeId, OfficeId, ReasonLost,
        StageChangedDate, NextActionDate, Notes, RelationshipLevelId, UrgencyLevelId,
        TechnicalFitId, BudgetLevelId, Probability, ServiceTypeId, ForecastDate, EstimatedValue,
        ForecastDealValue, Active, OpportunityName, Description, CreatedAt, UpdatedAt,
        DateCreation, Week, ContactCompany, TaxId, SegmentId, CountryCode, StateCode, City, Seller,
        DateActualStage, DaysOnStage, DateNextAction, ProbabilityPercent
    )
    VALUES
    (
        @NextOpportunityId,
        @ClientHorizonte, NULL, @StatusOpen, 180000.00, @CurrencyCode, CAST(DATEADD(DAY, -14, @Now) AS DATE), NULL,
        @LeadSourceId, @FunnelNegotiation, @CompanySizeId, @OfficeId, NULL,
        CAST(DATEADD(DAY, -2, @Now) AS DATE), CAST(DATEADD(DAY, 2, @Now) AS DATE), N'Cliente com urgência operacional',
        @RelationshipNew, @UrgencyHigh, @TechnicalMedium, @BudgetEstimated, NULL, @ServiceTypeId, CAST(DATEADD(DAY, 10, @Now) AS DATE), 180000.00,
        162000.00, 1, N'DUMMY - Parada de Manutenção Mina Norte', N'Reforço de mão de obra para parada de manutenção.', @Now, @Now,
        CAST(DATEADD(DAY, -14, @Now) AS DATE), DATEPART(ISO_WEEK, DATEADD(DAY, -14, @Now)), N'Horizonte Mineração', N'98.765.432/0001-10',
        @SegmentId, @CountryCode, @StateCode, N'Belo Horizonte', N'Ana Souza',
        CAST(DATEADD(DAY, -2, @Now) AS DATE), 2, CAST(DATEADD(DAY, 2, @Now) AS DATE), 69.0000
    );
    SET @NextOpportunityId = @NextOpportunityId + 1;
END;

IF NOT EXISTS (SELECT 1 FROM crm.Opportunity WHERE OpportunityName = 'DUMMY - Mobilização Offshore Q3')
BEGIN
    INSERT INTO crm.Opportunity
    (
        OpportunityId,
        ClientId, ContactId, StatusId, ValueAmount, CurrencyCode, OpenDate, CloseDate,
        LeadSourceId, FunnelStageId, CompanySizeId, OfficeId, ReasonLost,
        StageChangedDate, NextActionDate, Notes, RelationshipLevelId, UrgencyLevelId,
        TechnicalFitId, BudgetLevelId, Probability, ServiceTypeId, ForecastDate, EstimatedValue,
        ForecastDealValue, Active, OpportunityName, Description, CreatedAt, UpdatedAt,
        DateCreation, Week, ContactCompany, TaxId, SegmentId, CountryCode, StateCode, City, Seller,
        DateActualStage, DaysOnStage, DateNextAction, ProbabilityPercent
    )
    VALUES
    (
        @NextOpportunityId,
        @ClientSolaris, NULL, @StatusOpen, 320000.00, @CurrencyCode, CAST(DATEADD(DAY, -35, @Now) AS DATE), NULL,
        @LeadSourceId, @FunnelDeal, @CompanySizeId, @OfficeId, NULL,
        CAST(DATEADD(DAY, -1, @Now) AS DATE), CAST(DATEADD(DAY, 3, @Now) AS DATE), N'Negociação em fase final',
        @RelationshipRepeat, @UrgencyHigh, @TechnicalHigh, @BudgetApproved, NULL, @ServiceTypeId, CAST(DATEADD(DAY, 7, @Now) AS DATE), 320000.00,
        304000.00, 1, N'DUMMY - Mobilização Offshore Q3', N'Mobilização de time para campanha offshore.', @Now, @Now,
        CAST(DATEADD(DAY, -35, @Now) AS DATE), DATEPART(ISO_WEEK, DATEADD(DAY, -35, @Now)), N'Solaris Offshore', N'45.678.901/0001-33',
        @SegmentId, @CountryCode, @StateCode, N'Rio de Janeiro', N'Bruno Lima',
        CAST(DATEADD(DAY, -1, @Now) AS DATE), 1, CAST(DATEADD(DAY, 3, @Now) AS DATE), 96.0000
    );
    SET @NextOpportunityId = @NextOpportunityId + 1;
END;

IF NOT EXISTS (SELECT 1 FROM crm.Opportunity WHERE OpportunityName = 'DUMMY - Contrato Técnico Multi-site')
BEGIN
    INSERT INTO crm.Opportunity
    (
        OpportunityId,
        ClientId, ContactId, StatusId, ValueAmount, CurrencyCode, OpenDate, CloseDate,
        LeadSourceId, FunnelStageId, CompanySizeId, OfficeId, ReasonLost,
        StageChangedDate, NextActionDate, Notes, RelationshipLevelId, UrgencyLevelId,
        TechnicalFitId, BudgetLevelId, Probability, ServiceTypeId, ForecastDate, EstimatedValue,
        ForecastDealValue, Active, OpportunityName, Description, CreatedAt, UpdatedAt,
        DateCreation, Week, ContactCompany, TaxId, SegmentId, CountryCode, StateCode, City, Seller,
        DateActualStage, DaysOnStage, DateNextAction, ProbabilityPercent
    )
    VALUES
    (
        @NextOpportunityId,
        @ClientAtlas, NULL, @StatusOpen, 140000.00, @CurrencyCode, CAST(DATEADD(DAY, -10, @Now) AS DATE), NULL,
        @LeadSourceId, @FunnelLead, @CompanySizeId, @OfficeId, NULL,
        CAST(DATEADD(DAY, -6, @Now) AS DATE), CAST(DATEADD(DAY, 8, @Now) AS DATE), N'Necessita detalhamento de escopo',
        @RelationshipNew, @UrgencyLow, @TechnicalMedium, @BudgetEstimated, NULL, @ServiceTypeId, CAST(DATEADD(DAY, 25, @Now) AS DATE), 140000.00,
        126000.00, 1, N'DUMMY - Contrato Técnico Multi-site', N'Atendimento técnico para múltiplas unidades industriais.', @Now, @Now,
        CAST(DATEADD(DAY, -10, @Now) AS DATE), DATEPART(ISO_WEEK, DATEADD(DAY, -10, @Now)), N'Atlas Energia', N'12.345.678/0001-90',
        @SegmentId, @CountryCode, @StateCode, N'São Paulo', N'Carla Mendes',
        CAST(DATEADD(DAY, -6, @Now) AS DATE), 6, CAST(DATEADD(DAY, 8, @Now) AS DATE), 44.0000
    );
    SET @NextOpportunityId = @NextOpportunityId + 1;
END;

IF NOT EXISTS (SELECT 1 FROM crm.Opportunity WHERE OpportunityName = 'DUMMY - Renovação Contrato Base Leste')
BEGIN
    INSERT INTO crm.Opportunity
    (
        OpportunityId,
        ClientId, ContactId, StatusId, ValueAmount, CurrencyCode, OpenDate, CloseDate,
        LeadSourceId, FunnelStageId, CompanySizeId, OfficeId, ReasonLost,
        StageChangedDate, NextActionDate, Notes, RelationshipLevelId, UrgencyLevelId,
        TechnicalFitId, BudgetLevelId, Probability, ServiceTypeId, ForecastDate, EstimatedValue,
        ForecastDealValue, Active, OpportunityName, Description, CreatedAt, UpdatedAt,
        DateCreation, Week, ContactCompany, TaxId, SegmentId, CountryCode, StateCode, City, Seller,
        DateActualStage, DaysOnStage, DateNextAction, ProbabilityPercent
    )
    VALUES
    (
        @NextOpportunityId,
        @ClientHorizonte, NULL, @StatusWon, 110000.00, @CurrencyCode, CAST(DATEADD(DAY, -45, @Now) AS DATE), CAST(DATEADD(DAY, -3, @Now) AS DATE),
        @LeadSourceId, @FunnelDeal, @CompanySizeId, @OfficeId, NULL,
        CAST(DATEADD(DAY, -5, @Now) AS DATE), CAST(DATEADD(DAY, 1, @Now) AS DATE), N'Processo de renovação avançado',
        @RelationshipRepeat, @UrgencyLow, @TechnicalHigh, @BudgetApproved, NULL, @ServiceTypeId, CAST(DATEADD(DAY, 1, @Now) AS DATE), 110000.00,
        110000.00, 1, N'DUMMY - Renovação Contrato Base Leste', N'Renovação anual de contrato com ganho de escopo.', @Now, @Now,
        CAST(DATEADD(DAY, -45, @Now) AS DATE), DATEPART(ISO_WEEK, DATEADD(DAY, -45, @Now)), N'Horizonte Mineração', N'98.765.432/0001-10',
        @SegmentId, @CountryCode, @StateCode, N'Curitiba', N'Diego Alves',
        CAST(DATEADD(DAY, -5, @Now) AS DATE), 5, CAST(DATEADD(DAY, 1, @Now) AS DATE), 100.0000
    );
    SET @NextOpportunityId = @NextOpportunityId + 1;
END;

-- ---------------------------------------------------------------------------
-- 4) Insert proposals linked to dummy opportunities
-- ---------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM crm.Proposal WHERE Title = N'DUMMY - Proposta Expansão Subestação Sul')
BEGIN
    INSERT INTO crm.Proposal
    (
        ClientId, OpportunityId, Title, ObjectiveHtml, ProjectHours, GlobalMarginPercent,
        Status, TotalCost, TotalSellPrice, Active, CreatedAt, UpdatedAt
    )
    SELECT
        o.ClientId,
        o.OpportunityId,
        N'DUMMY - Proposta Expansão Subestação Sul',
        N'<p>Equipe dedicada para expansão da subestação sul.</p>',
        220.00,
        22.0000,
        N'Draft',
        0,
        0,
        1,
        @Now,
        @Now
    FROM crm.Opportunity o
    WHERE o.OpportunityName = N'DUMMY - Expansão Subestação Sul';
END;

IF NOT EXISTS (SELECT 1 FROM crm.Proposal WHERE Title = N'DUMMY - Proposta Parada Mina Norte')
BEGIN
    INSERT INTO crm.Proposal
    (
        ClientId, OpportunityId, Title, ObjectiveHtml, ProjectHours, GlobalMarginPercent,
        Status, TotalCost, TotalSellPrice, Active, CreatedAt, UpdatedAt
    )
    SELECT
        o.ClientId,
        o.OpportunityId,
        N'DUMMY - Proposta Parada Mina Norte',
        N'<p>Suporte operacional para parada de manutenção.</p>',
        220.00,
        18.0000,
        N'Draft',
        0,
        0,
        1,
        @Now,
        @Now
    FROM crm.Opportunity o
    WHERE o.OpportunityName = N'DUMMY - Parada de Manutenção Mina Norte';
END;

IF NOT EXISTS (SELECT 1 FROM crm.Proposal WHERE Title = N'DUMMY - Proposta Mobilização Offshore Q3')
BEGIN
    INSERT INTO crm.Proposal
    (
        ClientId, OpportunityId, Title, ObjectiveHtml, ProjectHours, GlobalMarginPercent,
        Status, TotalCost, TotalSellPrice, Active, CreatedAt, UpdatedAt
    )
    SELECT
        o.ClientId,
        o.OpportunityId,
        N'DUMMY - Proposta Mobilização Offshore Q3',
        N'<p>Mobilização offshore com equipe técnica completa.</p>',
        220.00,
        25.0000,
        N'Draft',
        0,
        0,
        1,
        @Now,
        @Now
    FROM crm.Opportunity o
    WHERE o.OpportunityName = N'DUMMY - Mobilização Offshore Q3';
END;

IF NOT EXISTS (SELECT 1 FROM crm.Proposal WHERE Title = N'DUMMY - Proposta Contrato Técnico Multi-site')
BEGIN
    INSERT INTO crm.Proposal
    (
        ClientId, OpportunityId, Title, ObjectiveHtml, ProjectHours, GlobalMarginPercent,
        Status, TotalCost, TotalSellPrice, Active, CreatedAt, UpdatedAt
    )
    SELECT
        o.ClientId,
        o.OpportunityId,
        N'DUMMY - Proposta Contrato Técnico Multi-site',
        N'<p>Prestação de serviços técnicos para múltiplas unidades.</p>',
        220.00,
        20.0000,
        N'Draft',
        0,
        0,
        1,
        @Now,
        @Now
    FROM crm.Opportunity o
    WHERE o.OpportunityName = N'DUMMY - Contrato Técnico Multi-site';
END;

IF NOT EXISTS (SELECT 1 FROM crm.Proposal WHERE Title = N'DUMMY - Proposta Renovação Base Leste')
BEGIN
    INSERT INTO crm.Proposal
    (
        ClientId, OpportunityId, Title, ObjectiveHtml, ProjectHours, GlobalMarginPercent,
        Status, TotalCost, TotalSellPrice, Active, CreatedAt, UpdatedAt
    )
    SELECT
        o.ClientId,
        o.OpportunityId,
        N'DUMMY - Proposta Renovação Base Leste',
        N'<p>Renovação contratual com incremento de escopo.</p>',
        220.00,
        15.0000,
        N'Draft',
        0,
        0,
        1,
        @Now,
        @Now
    FROM crm.Opportunity o
    WHERE o.OpportunityName = N'DUMMY - Renovação Contrato Base Leste';
END;

-- ---------------------------------------------------------------------------
-- 5) Link employees into proposals (for visual totals/tables)
-- ---------------------------------------------------------------------------
DECLARE @Emp1 INT = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE Active = 1 ORDER BY EmployeeId);
DECLARE @Emp2 INT = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE Active = 1 AND EmployeeId > ISNULL(@Emp1, 0) ORDER BY EmployeeId);
DECLARE @Emp3 INT = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE Active = 1 AND EmployeeId > ISNULL(@Emp2, 0) ORDER BY EmployeeId);
DECLARE @Emp4 INT = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE Active = 1 AND EmployeeId > ISNULL(@Emp3, 0) ORDER BY EmployeeId);
DECLARE @Emp5 INT = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE Active = 1 AND EmployeeId > ISNULL(@Emp4, 0) ORDER BY EmployeeId);

DECLARE @P1 INT = (SELECT TOP 1 ProposalId FROM crm.Proposal WHERE Title = N'DUMMY - Proposta Expansão Subestação Sul' ORDER BY ProposalId DESC);
DECLARE @P2 INT = (SELECT TOP 1 ProposalId FROM crm.Proposal WHERE Title = N'DUMMY - Proposta Parada Mina Norte' ORDER BY ProposalId DESC);
DECLARE @P3 INT = (SELECT TOP 1 ProposalId FROM crm.Proposal WHERE Title = N'DUMMY - Proposta Mobilização Offshore Q3' ORDER BY ProposalId DESC);
DECLARE @P4 INT = (SELECT TOP 1 ProposalId FROM crm.Proposal WHERE Title = N'DUMMY - Proposta Contrato Técnico Multi-site' ORDER BY ProposalId DESC);
DECLARE @P5 INT = (SELECT TOP 1 ProposalId FROM crm.Proposal WHERE Title = N'DUMMY - Proposta Renovação Base Leste' ORDER BY ProposalId DESC);

IF @P1 IS NOT NULL AND @Emp1 IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM crm.ProposalEmployee WHERE ProposalId = @P1 AND EmployeeId = @Emp1 AND Active = 1)
    INSERT INTO crm.ProposalEmployee (ProposalId, EmployeeId, CostSnapshot, MarginPercentApplied, SellPriceSnapshot, HourlyValueSnapshot, Active, CreatedAt, UpdatedAt)
    VALUES (@P1, @Emp1, 3500.00, 22.0000, 4270.00, ROUND(4270.00 / 220.0, 4), 1, @Now, @Now);

IF @P1 IS NOT NULL AND @Emp2 IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM crm.ProposalEmployee WHERE ProposalId = @P1 AND EmployeeId = @Emp2 AND Active = 1)
    INSERT INTO crm.ProposalEmployee (ProposalId, EmployeeId, CostSnapshot, MarginPercentApplied, SellPriceSnapshot, HourlyValueSnapshot, Active, CreatedAt, UpdatedAt)
    VALUES (@P1, @Emp2, 4200.00, 22.0000, 5124.00, ROUND(5124.00 / 220.0, 4), 1, @Now, @Now);

IF @P2 IS NOT NULL AND @Emp3 IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM crm.ProposalEmployee WHERE ProposalId = @P2 AND EmployeeId = @Emp3 AND Active = 1)
    INSERT INTO crm.ProposalEmployee (ProposalId, EmployeeId, CostSnapshot, MarginPercentApplied, SellPriceSnapshot, HourlyValueSnapshot, Active, CreatedAt, UpdatedAt)
    VALUES (@P2, @Emp3, 3800.00, 18.0000, 4484.00, ROUND(4484.00 / 220.0, 4), 1, @Now, @Now);

IF @P3 IS NOT NULL AND @Emp4 IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM crm.ProposalEmployee WHERE ProposalId = @P3 AND EmployeeId = @Emp4 AND Active = 1)
    INSERT INTO crm.ProposalEmployee (ProposalId, EmployeeId, CostSnapshot, MarginPercentApplied, SellPriceSnapshot, HourlyValueSnapshot, Active, CreatedAt, UpdatedAt)
    VALUES (@P3, @Emp4, 5600.00, 25.0000, 7000.00, ROUND(7000.00 / 220.0, 4), 1, @Now, @Now);

IF @P3 IS NOT NULL AND @Emp5 IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM crm.ProposalEmployee WHERE ProposalId = @P3 AND EmployeeId = @Emp5 AND Active = 1)
    INSERT INTO crm.ProposalEmployee (ProposalId, EmployeeId, CostSnapshot, MarginPercentApplied, SellPriceSnapshot, HourlyValueSnapshot, Active, CreatedAt, UpdatedAt)
    VALUES (@P3, @Emp5, 6100.00, 25.0000, 7625.00, ROUND(7625.00 / 220.0, 4), 1, @Now, @Now);

IF @P4 IS NOT NULL AND @Emp2 IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM crm.ProposalEmployee WHERE ProposalId = @P4 AND EmployeeId = @Emp2 AND Active = 1)
    INSERT INTO crm.ProposalEmployee (ProposalId, EmployeeId, CostSnapshot, MarginPercentApplied, SellPriceSnapshot, HourlyValueSnapshot, Active, CreatedAt, UpdatedAt)
    VALUES (@P4, @Emp2, 4100.00, 20.0000, 4920.00, ROUND(4920.00 / 220.0, 4), 1, @Now, @Now);

IF @P5 IS NOT NULL AND @Emp1 IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM crm.ProposalEmployee WHERE ProposalId = @P5 AND EmployeeId = @Emp1 AND Active = 1)
    INSERT INTO crm.ProposalEmployee (ProposalId, EmployeeId, CostSnapshot, MarginPercentApplied, SellPriceSnapshot, HourlyValueSnapshot, Active, CreatedAt, UpdatedAt)
    VALUES (@P5, @Emp1, 3300.00, 15.0000, 3795.00, ROUND(3795.00 / 220.0, 4), 1, @Now, @Now);

-- ---------------------------------------------------------------------------
-- 6) Recalculate proposal totals
-- ---------------------------------------------------------------------------
UPDATE p
SET
    p.TotalCost = ISNULL(t.TotalCost, 0),
    p.TotalSellPrice = ISNULL(t.TotalSellPrice, 0),
    p.UpdatedAt = @Now
FROM crm.Proposal p
OUTER APPLY (
    SELECT
        SUM(pe.CostSnapshot) AS TotalCost,
        SUM(pe.SellPriceSnapshot) AS TotalSellPrice
    FROM crm.ProposalEmployee pe
    WHERE pe.ProposalId = p.ProposalId
      AND pe.Active = 1
) t
WHERE p.Title LIKE N'DUMMY - Proposta%';

SELECT
    (SELECT COUNT(1) FROM crm.Opportunity WHERE OpportunityName LIKE N'DUMMY - %') AS DummyOpportunities,
    (SELECT COUNT(1) FROM crm.Proposal WHERE Title LIKE N'DUMMY - Proposta%') AS DummyProposals,
    (SELECT COUNT(1) FROM crm.ProposalEmployee pe INNER JOIN crm.Proposal p ON p.ProposalId = pe.ProposalId WHERE p.Title LIKE N'DUMMY - Proposta%' AND pe.Active = 1) AS DummyProposalEmployees;
