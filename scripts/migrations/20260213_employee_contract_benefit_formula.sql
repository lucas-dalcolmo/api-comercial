USE Apeiron_ONE;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('hr.EmployeeContractBenefit') AND name = 'IsFormula'
)
BEGIN
    ALTER TABLE hr.EmployeeContractBenefit
    ADD IsFormula BIT NOT NULL CONSTRAINT DF_EmployeeContractBenefit_IsFormula DEFAULT(0);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('hr.EmployeeContractBenefit') AND name = 'Formula'
)
BEGIN
    ALTER TABLE hr.EmployeeContractBenefit
    ADD Formula NVARCHAR(500) NULL;
END
GO

UPDATE hr.EmployeeContractBenefit
SET Formula = NULL
WHERE IsFormula = 0 AND Formula IS NOT NULL;
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_EmployeeContractBenefit_FormulaMode')
BEGIN
    ALTER TABLE hr.EmployeeContractBenefit DROP CONSTRAINT CK_EmployeeContractBenefit_FormulaMode;
END
GO

ALTER TABLE hr.EmployeeContractBenefit WITH CHECK
ADD CONSTRAINT CK_EmployeeContractBenefit_FormulaMode
CHECK (
    (IsFormula = 0 AND Formula IS NULL)
    OR (IsFormula = 1 AND Formula IS NOT NULL AND LEN(LTRIM(RTRIM(Formula))) > 0)
);
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_EmployeeContractBenefit_Contract_BenefitType_Active'
      AND object_id = OBJECT_ID('hr.EmployeeContractBenefit')
)
BEGIN
    CREATE UNIQUE INDEX UX_EmployeeContractBenefit_Contract_BenefitType_Active
    ON hr.EmployeeContractBenefit(ContractId, BenefitTypeId)
    WHERE Active = 1 AND BenefitTypeId IS NOT NULL;
END
GO

UPDATE b
SET
    b.IsFormula = 1,
    b.Formula = '[0.30*[EmployeeContract.BaseSalaryUsd]]',
    b.BenefitValue = NULL
FROM hr.EmployeeContractBenefit b
INNER JOIN crm.dm_BenefitType t ON t.BenefitTypeId = b.BenefitTypeId
WHERE t.BenefitTypeName = 'Periculosidade';
GO
