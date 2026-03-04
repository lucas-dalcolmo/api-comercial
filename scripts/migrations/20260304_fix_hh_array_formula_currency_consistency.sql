SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*
    Ensure HH array output in proposal documents always follows proposal currency.
    [ProposalEmployee.HourlyValueSnapshot] is already persisted in proposal currency
    (USD or BRL) during add/update/recompute flows.
*/
UPDATE crm.ProposalTemplateTagBinding
SET Formula = '[ProposalEmployee.HourlyValueSnapshot]',
    UpdatedAt = SYSUTCDATETIME()
WHERE Active = 1
  AND TagKey = 'HH'
  AND ValueMode = 'Array'
  AND (Formula IS NULL OR LTRIM(RTRIM(Formula)) <> '[ProposalEmployee.HourlyValueSnapshot]');
GO

