USE Apeiron_ONE;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

;WITH CategoryWeight AS
(
    SELECT
        MAX(CASE WHEN CategoryName = 'Funnel_Stage' AND Active = 1 THEN CategoryWeight END) AS FunnelCategoryWeight,
        MAX(CASE WHEN CategoryName = 'RelationShip_With_Client' AND Active = 1 THEN CategoryWeight END) AS RelationshipCategoryWeight,
        MAX(CASE WHEN CategoryName = 'Urgency' AND Active = 1 THEN CategoryWeight END) AS UrgencyCategoryWeight,
        MAX(CASE WHEN CategoryName = 'Technical_Fit' AND Active = 1 THEN CategoryWeight END) AS TechnicalCategoryWeight,
        MAX(CASE WHEN CategoryName = 'Budget' AND Active = 1 THEN CategoryWeight END) AS BudgetCategoryWeight
    FROM crm.ScoreCategoryWeight
),
OpportunityResolved AS
(
    SELECT
        o.OpportunityId,
        s.StatusName,
        fs.FunnelStageName,
        rl.RelationshipLevelName,
        ul.UrgencyLevelName,
        tf.TechnicalFitName,
        bl.BudgetLevelName
    FROM crm.Opportunity o
    LEFT JOIN crm.dm_OpportunityStatus s ON s.StatusId = o.StatusId
    LEFT JOIN crm.dm_FunnelStage fs ON fs.FunnelStageId = o.FunnelStageId
    LEFT JOIN crm.dm_RelationshipLevel rl ON rl.RelationshipLevelId = o.RelationshipLevelId
    LEFT JOIN crm.dm_UrgencyLevel ul ON ul.UrgencyLevelId = o.UrgencyLevelId
    LEFT JOIN crm.dm_TechnicalFit tf ON tf.TechnicalFitId = o.TechnicalFitId
    LEFT JOIN crm.dm_BudgetLevel bl ON bl.BudgetLevelId = o.BudgetLevelId
)
UPDATE o
SET
    ProbabilityPercent =
        CASE
            WHEN r.StatusName IN ('Closed - Lost', 'Lost') THEN CAST(0 AS DECIMAL(9,4))
            WHEN r.FunnelStageName = 'Deal' THEN CAST(100 AS DECIMAL(9,4))
            WHEN r.TechnicalFitName = 'Out of scope' THEN CAST(0 AS DECIMAL(9,4))
            WHEN r.FunnelStageName IS NULL
              OR r.RelationshipLevelName IS NULL
              OR r.UrgencyLevelName IS NULL
              OR r.TechnicalFitName IS NULL
              OR r.BudgetLevelName IS NULL THEN NULL
            WHEN cw.FunnelCategoryWeight IS NULL
              OR cw.RelationshipCategoryWeight IS NULL
              OR cw.UrgencyCategoryWeight IS NULL
              OR cw.TechnicalCategoryWeight IS NULL
              OR cw.BudgetCategoryWeight IS NULL THEN NULL
            WHEN vw.FunnelValueWeight IS NULL
              OR vw.RelationshipValueWeight IS NULL
              OR vw.UrgencyValueWeight IS NULL
              OR vw.TechnicalValueWeight IS NULL
              OR vw.BudgetValueWeight IS NULL THEN NULL
            ELSE
                ROUND(
                    (vw.FunnelValueWeight * cw.FunnelCategoryWeight)
                  + (vw.RelationshipValueWeight * cw.RelationshipCategoryWeight)
                  + (vw.UrgencyValueWeight * cw.UrgencyCategoryWeight)
                  + (vw.TechnicalValueWeight * cw.TechnicalCategoryWeight)
                  + (vw.BudgetValueWeight * cw.BudgetCategoryWeight),
                    4
                )
        END,
    UpdatedAt = SYSUTCDATETIME()
FROM crm.Opportunity o
INNER JOIN OpportunityResolved r ON r.OpportunityId = o.OpportunityId
CROSS JOIN CategoryWeight cw
OUTER APPLY
(
    SELECT
        (
            SELECT TOP 1 v.ValueWeight
            FROM crm.ScoreValueWeight v
            WHERE v.Active = 1
              AND v.CategoryName = 'Funnel_Stage'
              AND v.ValueName = r.FunnelStageName
        ) AS FunnelValueWeight,
        (
            SELECT TOP 1 v.ValueWeight
            FROM crm.ScoreValueWeight v
            WHERE v.Active = 1
              AND v.CategoryName = 'RelationShip_With_Client'
              AND v.ValueName = r.RelationshipLevelName
        ) AS RelationshipValueWeight,
        (
            SELECT TOP 1 v.ValueWeight
            FROM crm.ScoreValueWeight v
            WHERE v.Active = 1
              AND v.CategoryName = 'Urgency'
              AND v.ValueName = r.UrgencyLevelName
        ) AS UrgencyValueWeight,
        (
            SELECT TOP 1 v.ValueWeight
            FROM crm.ScoreValueWeight v
            WHERE v.Active = 1
              AND v.CategoryName = 'Technical_Fit'
              AND v.ValueName = r.TechnicalFitName
        ) AS TechnicalValueWeight,
        (
            SELECT TOP 1 v.ValueWeight
            FROM crm.ScoreValueWeight v
            WHERE v.Active = 1
              AND v.CategoryName = 'Budget'
              AND v.ValueName = r.BudgetLevelName
        ) AS BudgetValueWeight
) vw;

SELECT OpportunityId, OpportunityName, ProbabilityPercent
FROM crm.Opportunity
ORDER BY OpportunityId DESC;
GO
