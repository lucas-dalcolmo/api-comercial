USE Apeiron_ONE;
GO

IF OBJECT_ID('hr.BenefitFormulaVariable','U') IS NULL
BEGIN
    CREATE TABLE hr.BenefitFormulaVariable (
        BenefitFormulaVariableId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        VariableKey NVARCHAR(150) NOT NULL,
        SourceScope NVARCHAR(40) NOT NULL,
        SourceSchema NVARCHAR(30) NOT NULL,
        SourceTable NVARCHAR(128) NOT NULL,
        SourceColumn NVARCHAR(128) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_BenefitFormulaVariable_Active DEFAULT(1)
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_BenefitFormulaVariable_Key'
      AND object_id = OBJECT_ID('hr.BenefitFormulaVariable')
)
BEGIN
    CREATE UNIQUE INDEX UX_BenefitFormulaVariable_Key ON hr.BenefitFormulaVariable(VariableKey);
END
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_BenefitFormulaVariable_SourceScope')
BEGIN
    ALTER TABLE hr.BenefitFormulaVariable DROP CONSTRAINT CK_BenefitFormulaVariable_SourceScope;
END
GO

ALTER TABLE hr.BenefitFormulaVariable WITH CHECK
ADD CONSTRAINT CK_BenefitFormulaVariable_SourceScope
CHECK (SourceScope IN ('Contract', 'EmployeeFromContract'));
GO

IF NOT EXISTS (SELECT 1 FROM hr.BenefitFormulaVariable WHERE VariableKey = 'EmployeeContract.BaseSalaryUsd')
BEGIN
    INSERT INTO hr.BenefitFormulaVariable (VariableKey, SourceScope, SourceSchema, SourceTable, SourceColumn, Active)
    VALUES ('EmployeeContract.BaseSalaryUsd', 'Contract', 'hr', 'EmployeeContract', 'BaseSalaryUsd', 1);
END
GO
