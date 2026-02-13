IF DB_ID('Apeiron_ONE') IS NULL
    CREATE DATABASE Apeiron_ONE;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
USE Apeiron_ONE;
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'crm')
    EXEC('CREATE SCHEMA crm');
GO

-- Helper: add Active column if missing
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql = @sql + 'IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(''' + s.name + '.' + t.name + ''') AND name = ''Active'') ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' ADD Active BIT NOT NULL CONSTRAINT DF_' + t.name + '_Active DEFAULT(1);' + CHAR(10)
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = 'crm' AND t.name LIKE 'dm_%';
EXEC sp_executesql @sql;
GO

-- IntLookup tables
IF OBJECT_ID('crm.dm_BloodType','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_BloodType (
        BloodTypeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        BloodTypeName VARCHAR(5) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_BloodType_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_BloodType_Name' AND object_id = OBJECT_ID('crm.dm_BloodType'))
    CREATE UNIQUE INDEX UX_dm_BloodType_Name ON crm.dm_BloodType(BloodTypeName);
GO

IF OBJECT_ID('crm.dm_BudgetLevel','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_BudgetLevel (
        BudgetLevelId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        BudgetLevelName VARCHAR(60) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_BudgetLevel_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_BudgetLevel_Name' AND object_id = OBJECT_ID('crm.dm_BudgetLevel'))
    CREATE UNIQUE INDEX UX_dm_BudgetLevel_Name ON crm.dm_BudgetLevel(BudgetLevelName);
GO

IF OBJECT_ID('crm.dm_CompanySize','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_CompanySize (
        CompanySizeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CompanySizeName VARCHAR(30) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_CompanySize_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_CompanySize_Name' AND object_id = OBJECT_ID('crm.dm_CompanySize'))
    CREATE UNIQUE INDEX UX_dm_CompanySize_Name ON crm.dm_CompanySize(CompanySizeName);
GO

IF OBJECT_ID('crm.dm_Department','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_Department (
        DepartmentId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        DepartmentName VARCHAR(80) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_Department_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_Department_Name' AND object_id = OBJECT_ID('crm.dm_Department'))
    CREATE UNIQUE INDEX UX_dm_Department_Name ON crm.dm_Department(DepartmentName);
GO

IF OBJECT_ID('crm.dm_EducationLevel','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_EducationLevel (
        EducationLevelId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EducationLevelName VARCHAR(80) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_EducationLevel_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_EducationLevel_Name' AND object_id = OBJECT_ID('crm.dm_EducationLevel'))
    CREATE UNIQUE INDEX UX_dm_EducationLevel_Name ON crm.dm_EducationLevel(EducationLevelName);
GO

IF OBJECT_ID('crm.dm_EmploymentType','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_EmploymentType (
        EmploymentTypeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EmploymentTypeName VARCHAR(20) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_EmploymentType_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_EmploymentType_Name' AND object_id = OBJECT_ID('crm.dm_EmploymentType'))
    CREATE UNIQUE INDEX UX_dm_EmploymentType_Name ON crm.dm_EmploymentType(EmploymentTypeName);
GO

IF OBJECT_ID('crm.dm_FunnelStage','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_FunnelStage (
        FunnelStageId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FunnelStageName VARCHAR(60) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_FunnelStage_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_FunnelStage_Name' AND object_id = OBJECT_ID('crm.dm_FunnelStage'))
    CREATE UNIQUE INDEX UX_dm_FunnelStage_Name ON crm.dm_FunnelStage(FunnelStageName);
GO

IF OBJECT_ID('crm.dm_Gender','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_Gender (
        GenderId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        GenderName VARCHAR(30) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_Gender_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_Gender_Name' AND object_id = OBJECT_ID('crm.dm_Gender'))
    CREATE UNIQUE INDEX UX_dm_Gender_Name ON crm.dm_Gender(GenderName);
GO

IF OBJECT_ID('crm.dm_InteractionType','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_InteractionType (
        InteractionTypeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        InteractionTypeName VARCHAR(50) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_InteractionType_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_InteractionType_Name' AND object_id = OBJECT_ID('crm.dm_InteractionType'))
    CREATE UNIQUE INDEX UX_dm_InteractionType_Name ON crm.dm_InteractionType(InteractionTypeName);
GO

IF OBJECT_ID('crm.dm_LeadSource','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_LeadSource (
        LeadSourceId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LeadSourceName VARCHAR(50) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_LeadSource_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_LeadSource_Name' AND object_id = OBJECT_ID('crm.dm_LeadSource'))
    CREATE UNIQUE INDEX UX_dm_LeadSource_Name ON crm.dm_LeadSource(LeadSourceName);
GO

IF OBJECT_ID('crm.dm_MaritalStatus','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_MaritalStatus (
        MaritalStatusId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaritalStatusName VARCHAR(40) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_MaritalStatus_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_MaritalStatus_Name' AND object_id = OBJECT_ID('crm.dm_MaritalStatus'))
    CREATE UNIQUE INDEX UX_dm_MaritalStatus_Name ON crm.dm_MaritalStatus(MaritalStatusName);
GO

IF OBJECT_ID('crm.dm_Office','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_Office (
        OfficeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        OfficeName VARCHAR(80) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_Office_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_Office_Name' AND object_id = OBJECT_ID('crm.dm_Office'))
    CREATE UNIQUE INDEX UX_dm_Office_Name ON crm.dm_Office(OfficeName);
GO

IF OBJECT_ID('crm.dm_OpportunityStatus','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_OpportunityStatus (
        StatusId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        StatusName VARCHAR(50) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_OpportunityStatus_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_OpportunityStatus_Name' AND object_id = OBJECT_ID('crm.dm_OpportunityStatus'))
    CREATE UNIQUE INDEX UX_dm_OpportunityStatus_Name ON crm.dm_OpportunityStatus(StatusName);
GO

IF OBJECT_ID('crm.dm_Region','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_Region (
        RegionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RegionName VARCHAR(80) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_Region_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_Region_Name' AND object_id = OBJECT_ID('crm.dm_Region'))
    CREATE UNIQUE INDEX UX_dm_Region_Name ON crm.dm_Region(RegionName);
GO

IF OBJECT_ID('crm.dm_RelationshipLevel','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_RelationshipLevel (
        RelationshipLevelId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RelationshipLevelName VARCHAR(60) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_RelationshipLevel_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_RelationshipLevel_Name' AND object_id = OBJECT_ID('crm.dm_RelationshipLevel'))
    CREATE UNIQUE INDEX UX_dm_RelationshipLevel_Name ON crm.dm_RelationshipLevel(RelationshipLevelName);
GO

IF OBJECT_ID('crm.dm_Role','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_Role (
        RoleId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RoleName VARCHAR(120) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_Role_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_Role_Name' AND object_id = OBJECT_ID('crm.dm_Role'))
    CREATE UNIQUE INDEX UX_dm_Role_Name ON crm.dm_Role(RoleName);
GO

IF OBJECT_ID('crm.dm_Segment','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_Segment (
        SegmentId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SegmentName VARCHAR(100) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_Segment_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_Segment_Name' AND object_id = OBJECT_ID('crm.dm_Segment'))
    CREATE UNIQUE INDEX UX_dm_Segment_Name ON crm.dm_Segment(SegmentName);
GO

IF OBJECT_ID('crm.dm_ServiceType','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_ServiceType (
        ServiceTypeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ServiceTypeName VARCHAR(80) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_ServiceType_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_ServiceType_Name' AND object_id = OBJECT_ID('crm.dm_ServiceType'))
    CREATE UNIQUE INDEX UX_dm_ServiceType_Name ON crm.dm_ServiceType(ServiceTypeName);
GO

IF OBJECT_ID('crm.dm_TechnicalFit','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_TechnicalFit (
        TechnicalFitId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TechnicalFitName VARCHAR(60) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_TechnicalFit_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_TechnicalFit_Name' AND object_id = OBJECT_ID('crm.dm_TechnicalFit'))
    CREATE UNIQUE INDEX UX_dm_TechnicalFit_Name ON crm.dm_TechnicalFit(TechnicalFitName);
GO

IF OBJECT_ID('crm.dm_UrgencyLevel','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_UrgencyLevel (
        UrgencyLevelId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UrgencyLevelName VARCHAR(40) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_UrgencyLevel_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_UrgencyLevel_Name' AND object_id = OBJECT_ID('crm.dm_UrgencyLevel'))
    CREATE UNIQUE INDEX UX_dm_UrgencyLevel_Name ON crm.dm_UrgencyLevel(UrgencyLevelName);
GO

IF OBJECT_ID('crm.dm_DocumentType','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_DocumentType (
        DocumentTypeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        DocumentTypeName VARCHAR(80) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_DocumentType_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_DocumentType_Name' AND object_id = OBJECT_ID('crm.dm_DocumentType'))
    CREATE UNIQUE INDEX UX_dm_DocumentType_Name ON crm.dm_DocumentType(DocumentTypeName);
GO

IF OBJECT_ID('crm.dm_BenefitType','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_BenefitType (
        BenefitTypeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        BenefitTypeName VARCHAR(80) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_BenefitType_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_BenefitType_Name' AND object_id = OBJECT_ID('crm.dm_BenefitType'))
    CREATE UNIQUE INDEX UX_dm_BenefitType_Name ON crm.dm_BenefitType(BenefitTypeName);
GO
IF NOT EXISTS (SELECT 1 FROM crm.dm_BenefitType)
BEGIN
    INSERT INTO crm.dm_BenefitType (BenefitTypeName) VALUES
        ('Ticket Alimentação'),
        ('Transporte'),
        ('Periculosidade');
END
GO

IF OBJECT_ID('crm.dm_Nationality','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_Nationality (
        NationalityId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        NationalityName VARCHAR(80) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_Nationality_Active DEFAULT(1)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_dm_Nationality_Name' AND object_id = OBJECT_ID('crm.dm_Nationality'))
    CREATE UNIQUE INDEX UX_dm_Nationality_Name ON crm.dm_Nationality(NationalityName);
GO
IF NOT EXISTS (SELECT 1 FROM crm.dm_Nationality)
BEGIN
    INSERT INTO crm.dm_Nationality (NationalityName) VALUES
        ('Brasileira(o)'),
        ('Portuguesa(o)'),
        ('Boliviana(o)'),
        ('Paraguaia(o)'),
        ('Haitiana(o)'),
        ('Argentina(o)'),
        ('Japonesa(o)'),
        ('Italiana(o)'),
        ('Chinesa(o)'),
        ('Estadunidense'),
        ('Espanhola(o)'),
        ('Venezuelana(o)');
END
GO

-- CodeName tables
IF OBJECT_ID('crm.dm_Country','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_Country (
        CountryCode CHAR(2) NOT NULL PRIMARY KEY,
        CountryName VARCHAR(80) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_Country_Active DEFAULT(1)
    );
END
GO

IF OBJECT_ID('crm.dm_Currency','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_Currency (
        CurrencyCode CHAR(3) NOT NULL PRIMARY KEY,
        CurrencyName VARCHAR(50) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_Currency_Active DEFAULT(1)
    );
END
GO

-- State table
IF OBJECT_ID('crm.dm_State','U') IS NULL
BEGIN
    CREATE TABLE crm.dm_State (
        StateCode CHAR(2) NOT NULL PRIMARY KEY,
        CountryCode CHAR(2) NOT NULL,
        StateName VARCHAR(80) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_dm_State_Active DEFAULT(1),
        CONSTRAINT FK_State_Country FOREIGN KEY (CountryCode) REFERENCES crm.dm_Country(CountryCode)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'hr')
    EXEC('CREATE SCHEMA hr');
GO

IF OBJECT_ID('hr.Employee','U') IS NULL
BEGIN
    CREATE TABLE hr.Employee (
        EmployeeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FullName NVARCHAR(200) NOT NULL,
        CPF NVARCHAR(14) NULL,
        GenderId INT NULL,
        BirthDate DATE NULL,
        Nationality NVARCHAR(80) NULL,
        PlaceOfBirth NVARCHAR(120) NULL,
        MaritalStatusId INT NULL,
        ChildrenCount INT NULL,
        Phone NVARCHAR(30) NULL,
        PersonalEmail NVARCHAR(255) NULL,
        CorporateEmail NVARCHAR(255) NULL,
        Address NVARCHAR(300) NULL,
        EducationLevelId INT NULL,
        BloodTypeId INT NULL,
        HireDate DATE NULL,
        Active BIT NOT NULL CONSTRAINT DF_Employee_Active DEFAULT(1)
    );
END
GO

IF OBJECT_ID('hr.EmployeeContract','U') IS NULL
BEGIN
    CREATE TABLE hr.EmployeeContract (
        ContractId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EmployeeId INT NULL,
        EmploymentTypeId INT NULL,
        CNPJ NVARCHAR(18) NULL,
        RoleId INT NULL,
        DepartmentId INT NULL,
        RegionId INT NULL,
        OfficeId INT NULL,
        BaseSalaryUsd DECIMAL(18,2) NULL,
        StartDate DATE NULL,
        EndDate DATE NULL,
        Active BIT NOT NULL CONSTRAINT DF_EmployeeContract_Active DEFAULT(1),
        CONSTRAINT FK_EmployeeContract_Employee FOREIGN KEY (EmployeeId) REFERENCES hr.Employee(EmployeeId)
    );
END
GO
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('hr.EmployeeContract') AND name = 'BaseSalaryUsd'
)
    ALTER TABLE hr.EmployeeContract ADD BaseSalaryUsd DECIMAL(18,2) NULL;
GO
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('hr.EmployeeContract') AND name = 'EmployeeId'
)
    ALTER TABLE hr.EmployeeContract ALTER COLUMN EmployeeId INT NULL;
GO

IF OBJECT_ID('hr.EmployeeDocument','U') IS NULL
BEGIN
    CREATE TABLE hr.EmployeeDocument (
        DocumentId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EmployeeId INT NOT NULL,
        DocumentTypeId INT NULL,
        DocumentNumber NVARCHAR(120) NULL,
        CountryCode CHAR(2) NULL,
        IssueDate DATE NULL,
        ExpiryDate DATE NULL,
        Notes NVARCHAR(500) NULL,
        Active BIT NOT NULL CONSTRAINT DF_EmployeeDocument_Active DEFAULT(1),
        CONSTRAINT FK_EmployeeDocument_Employee FOREIGN KEY (EmployeeId) REFERENCES hr.Employee(EmployeeId)
    );
END
GO
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('hr.EmployeeDocument') AND name = 'CountryCode'
)
    ALTER TABLE hr.EmployeeDocument ADD CountryCode CHAR(2) NULL;
GO

IF OBJECT_ID('hr.EmployeeBenefit','U') IS NULL
BEGIN
    CREATE TABLE hr.EmployeeBenefit (
        BenefitId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EmployeeId INT NOT NULL,
        BenefitTypeId INT NULL,
        StartDate DATE NULL,
        EndDate DATE NULL,
        Active BIT NOT NULL CONSTRAINT DF_EmployeeBenefit_Active DEFAULT(1),
        CONSTRAINT FK_EmployeeBenefit_Employee FOREIGN KEY (EmployeeId) REFERENCES hr.Employee(EmployeeId)
    );
END
GO

IF OBJECT_ID('hr.EmployeeContractBenefit','U') IS NULL
BEGIN
    CREATE TABLE hr.EmployeeContractBenefit (
        ContractBenefitId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ContractId INT NOT NULL,
        BenefitTypeId INT NULL,
        BenefitValue DECIMAL(18,2) NULL,
        IsFormula BIT NOT NULL CONSTRAINT DF_EmployeeContractBenefit_IsFormula DEFAULT(0),
        Formula NVARCHAR(500) NULL,
        Active BIT NOT NULL CONSTRAINT DF_EmployeeContractBenefit_Active DEFAULT(1),
        CONSTRAINT FK_EmployeeContractBenefit_Contract FOREIGN KEY (ContractId) REFERENCES hr.EmployeeContract(ContractId)
    );
END
GO
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('hr.EmployeeContractBenefit') AND name = 'IsFormula'
)
    ALTER TABLE hr.EmployeeContractBenefit
    ADD IsFormula BIT NOT NULL CONSTRAINT DF_EmployeeContractBenefit_IsFormula DEFAULT(0);
GO
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('hr.EmployeeContractBenefit') AND name = 'Formula'
)
    ALTER TABLE hr.EmployeeContractBenefit
    ADD Formula NVARCHAR(500) NULL;
GO
UPDATE hr.EmployeeContractBenefit
SET Formula = NULL
WHERE IsFormula = 0 AND Formula IS NOT NULL;
GO
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_EmployeeContractBenefit_FormulaMode')
    ALTER TABLE hr.EmployeeContractBenefit DROP CONSTRAINT CK_EmployeeContractBenefit_FormulaMode;
GO
ALTER TABLE hr.EmployeeContractBenefit WITH CHECK
ADD CONSTRAINT CK_EmployeeContractBenefit_FormulaMode
CHECK (
    (IsFormula = 0 AND Formula IS NULL)
    OR (IsFormula = 1 AND Formula IS NOT NULL AND LEN(LTRIM(RTRIM(Formula))) > 0)
);
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
    CREATE UNIQUE INDEX UX_BenefitFormulaVariable_Key ON hr.BenefitFormulaVariable(VariableKey);
GO
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_BenefitFormulaVariable_SourceScope')
    ALTER TABLE hr.BenefitFormulaVariable DROP CONSTRAINT CK_BenefitFormulaVariable_SourceScope;
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

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employee_Gender')
    ALTER TABLE hr.Employee ADD CONSTRAINT FK_Employee_Gender FOREIGN KEY (GenderId) REFERENCES crm.dm_Gender(GenderId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employee_MaritalStatus')
    ALTER TABLE hr.Employee ADD CONSTRAINT FK_Employee_MaritalStatus FOREIGN KEY (MaritalStatusId) REFERENCES crm.dm_MaritalStatus(MaritalStatusId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employee_EducationLevel')
    ALTER TABLE hr.Employee ADD CONSTRAINT FK_Employee_EducationLevel FOREIGN KEY (EducationLevelId) REFERENCES crm.dm_EducationLevel(EducationLevelId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employee_BloodType')
    ALTER TABLE hr.Employee ADD CONSTRAINT FK_Employee_BloodType FOREIGN KEY (BloodTypeId) REFERENCES crm.dm_BloodType(BloodTypeId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EmployeeContract_EmploymentType')
    ALTER TABLE hr.EmployeeContract ADD CONSTRAINT FK_EmployeeContract_EmploymentType FOREIGN KEY (EmploymentTypeId) REFERENCES crm.dm_EmploymentType(EmploymentTypeId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EmployeeContract_Role')
    ALTER TABLE hr.EmployeeContract ADD CONSTRAINT FK_EmployeeContract_Role FOREIGN KEY (RoleId) REFERENCES crm.dm_Role(RoleId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EmployeeContract_Department')
    ALTER TABLE hr.EmployeeContract ADD CONSTRAINT FK_EmployeeContract_Department FOREIGN KEY (DepartmentId) REFERENCES crm.dm_Department(DepartmentId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EmployeeContract_Region')
    ALTER TABLE hr.EmployeeContract ADD CONSTRAINT FK_EmployeeContract_Region FOREIGN KEY (RegionId) REFERENCES crm.dm_Region(RegionId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EmployeeContract_Office')
    ALTER TABLE hr.EmployeeContract ADD CONSTRAINT FK_EmployeeContract_Office FOREIGN KEY (OfficeId) REFERENCES crm.dm_Office(OfficeId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EmployeeDocument_DocumentType')
    ALTER TABLE hr.EmployeeDocument ADD CONSTRAINT FK_EmployeeDocument_DocumentType FOREIGN KEY (DocumentTypeId) REFERENCES crm.dm_DocumentType(DocumentTypeId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EmployeeBenefit_BenefitType')
    ALTER TABLE hr.EmployeeBenefit ADD CONSTRAINT FK_EmployeeBenefit_BenefitType FOREIGN KEY (BenefitTypeId) REFERENCES crm.dm_BenefitType(BenefitTypeId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EmployeeContractBenefit_BenefitType')
    ALTER TABLE hr.EmployeeContractBenefit ADD CONSTRAINT FK_EmployeeContractBenefit_BenefitType FOREIGN KEY (BenefitTypeId) REFERENCES crm.dm_BenefitType(BenefitTypeId);
GO
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_EmployeeContractBenefit_Contract_BenefitType_Active'
      AND object_id = OBJECT_ID('hr.EmployeeContractBenefit')
)
    CREATE UNIQUE INDEX UX_EmployeeContractBenefit_Contract_BenefitType_Active
    ON hr.EmployeeContractBenefit(ContractId, BenefitTypeId)
    WHERE Active = 1 AND BenefitTypeId IS NOT NULL;
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

-- Score tables
IF OBJECT_ID('crm.ScoreCategoryWeight','U') IS NULL
BEGIN
    CREATE TABLE crm.ScoreCategoryWeight (
        CategoryName NVARCHAR(60) NOT NULL PRIMARY KEY,
        CategoryWeight DECIMAL(9,4) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_ScoreCategoryWeight_Active DEFAULT(1)
    );
END
ELSE
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('crm.ScoreCategoryWeight') AND name = 'Active'
    )
        ALTER TABLE crm.ScoreCategoryWeight ADD Active BIT NOT NULL CONSTRAINT DF_ScoreCategoryWeight_Active DEFAULT(1);
END
GO

IF OBJECT_ID('crm.ScoreValueWeight','U') IS NULL
BEGIN
    CREATE TABLE crm.ScoreValueWeight (
        CategoryName NVARCHAR(60) NOT NULL,
        ValueName NVARCHAR(100) NOT NULL,
        ValueWeight DECIMAL(9,4) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_ScoreValueWeight_Active DEFAULT(1),
        CONSTRAINT PK_ScoreValueWeight PRIMARY KEY (CategoryName, ValueName),
        CONSTRAINT FK_ScoreValueWeight_Category FOREIGN KEY (CategoryName) REFERENCES crm.ScoreCategoryWeight(CategoryName)
    );
END
ELSE
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('crm.ScoreValueWeight') AND name = 'Active'
    )
        ALTER TABLE crm.ScoreValueWeight ADD Active BIT NOT NULL CONSTRAINT DF_ScoreValueWeight_Active DEFAULT(1);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_ScoreCategoryWeight_Value')
    ALTER TABLE crm.ScoreCategoryWeight WITH CHECK ADD CONSTRAINT CK_ScoreCategoryWeight_Value CHECK (CategoryWeight >= 0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_ScoreValueWeight_Value')
    ALTER TABLE crm.ScoreValueWeight WITH CHECK ADD CONSTRAINT CK_ScoreValueWeight_Value CHECK (ValueWeight >= 0);
GO

-- CRM Client and Proposal tables
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

IF OBJECT_ID('crm.Proposal','U') IS NULL
BEGIN
    CREATE TABLE crm.Proposal (
        ProposalId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClientId INT NOT NULL,
        OpportunityId INT NULL,
        Title NVARCHAR(200) NOT NULL,
        ObjectiveHtml NVARCHAR(MAX) NOT NULL,
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

IF OBJECT_ID('crm.ProposalEmployee','U') IS NULL
BEGIN
    CREATE TABLE crm.ProposalEmployee (
        ProposalEmployeeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProposalId INT NOT NULL,
        EmployeeId INT NOT NULL,
        CostSnapshot DECIMAL(18,2) NOT NULL,
        MarginPercentApplied DECIMAL(9,4) NOT NULL,
        SellPriceSnapshot DECIMAL(18,2) NOT NULL,
        Active BIT NOT NULL CONSTRAINT DF_ProposalEmployee_Active DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProposalEmployee_CreatedAt DEFAULT(SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProposalEmployee_UpdatedAt DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_ProposalEmployee_Proposal FOREIGN KEY (ProposalId) REFERENCES crm.Proposal(ProposalId),
        CONSTRAINT FK_ProposalEmployee_Employee FOREIGN KEY (EmployeeId) REFERENCES hr.Employee(EmployeeId)
    );
END
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
    SELECT 1 FROM crm.Client WHERE ClientName = 'Fortlev Demo Client'
)
BEGIN
    DECLARE @NextClientId INT = (SELECT ISNULL(MAX(ClientId), 0) + 1 FROM crm.Client);
    INSERT INTO crm.Client (ClientId, ClientName, LegalName, LogoUrl, Active)
    VALUES (@NextClientId, 'Fortlev Demo Client', 'Fortlev Demo Client LTDA', '/uploads/client-logos/demo-client.png', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM hr.Employee WHERE FullName = 'Alice Demo')
BEGIN
    INSERT INTO hr.Employee (FullName, CPF, Active)
    VALUES ('Alice Demo', '000.000.000-01', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM hr.Employee WHERE FullName = 'Bruno Demo')
BEGIN
    INSERT INTO hr.Employee (FullName, CPF, Active)
    VALUES ('Bruno Demo', '000.000.000-02', 1);
END
GO

DECLARE @AliceId INT = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Alice Demo' ORDER BY EmployeeId DESC);
DECLARE @BrunoId INT = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Bruno Demo' ORDER BY EmployeeId DESC);

IF @AliceId IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM hr.EmployeeContract WHERE EmployeeId = @AliceId AND Active = 1
)
BEGIN
    INSERT INTO hr.EmployeeContract (EmployeeId, BaseSalaryUsd, StartDate, Active)
    VALUES (@AliceId, 3000.00, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO

DECLARE @AliceContractId INT = (
    SELECT TOP 1 ContractId
    FROM hr.EmployeeContract
    WHERE EmployeeId = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Alice Demo' ORDER BY EmployeeId DESC)
      AND Active = 1
    ORDER BY StartDate DESC, ContractId DESC
);

IF @AliceContractId IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM hr.EmployeeContractBenefit WHERE ContractId = @AliceContractId AND Active = 1
)
BEGIN
    INSERT INTO hr.EmployeeContractBenefit (ContractId, BenefitTypeId, BenefitValue, IsFormula, Formula, Active)
    VALUES (@AliceContractId, NULL, 300.00, 0, NULL, 1);
END
GO

IF @BrunoId IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM hr.EmployeeContract WHERE EmployeeId = @BrunoId AND Active = 1
)
BEGIN
    INSERT INTO hr.EmployeeContract (EmployeeId, BaseSalaryUsd, StartDate, Active)
    VALUES (@BrunoId, 5000.00, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO

DECLARE @DemoClientId INT = (SELECT TOP 1 ClientId FROM crm.Client WHERE ClientName = 'Fortlev Demo Client' ORDER BY ClientId DESC);
IF @DemoClientId IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM crm.Proposal WHERE ClientId = @DemoClientId AND Title = 'Proposta Comercial Demo'
)
BEGIN
    INSERT INTO crm.Proposal
    (
        ClientId,
        OpportunityId,
        Title,
        ObjectiveHtml,
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
        @DemoClientId,
        NULL,
        'Proposta Comercial Demo',
        '<p><strong>Objetivo:</strong> fornecer time dedicado com margem global de 20%.</p>',
        20.0000,
        'Draft',
        9960.00,
        11952.00,
        1,
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    );
END
GO

DECLARE @DemoProposalId INT = (SELECT TOP 1 ProposalId FROM crm.Proposal WHERE Title = 'Proposta Comercial Demo' ORDER BY ProposalId DESC);
DECLARE @AliceCost DECIMAL(18,2) = (
    SELECT TOP 1 ISNULL(c.BaseSalaryUsd, 0) + ISNULL((
        SELECT SUM(ISNULL(b.BenefitValue, 0))
        FROM hr.EmployeeContractBenefit b
        WHERE b.ContractId = c.ContractId AND b.Active = 1 AND b.IsFormula = 0
    ), 0)
    FROM hr.EmployeeContract c
    WHERE c.EmployeeId = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Alice Demo' ORDER BY EmployeeId DESC)
      AND c.Active = 1
    ORDER BY c.StartDate DESC, c.ContractId DESC
);
DECLARE @BrunoCost DECIMAL(18,2) = (
    SELECT TOP 1 ISNULL(c.BaseSalaryUsd, 0) + ISNULL((
        SELECT SUM(ISNULL(b.BenefitValue, 0))
        FROM hr.EmployeeContractBenefit b
        WHERE b.ContractId = c.ContractId AND b.Active = 1 AND b.IsFormula = 0
    ), 0)
    FROM hr.EmployeeContract c
    WHERE c.EmployeeId = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Bruno Demo' ORDER BY EmployeeId DESC)
      AND c.Active = 1
    ORDER BY c.StartDate DESC, c.ContractId DESC
);

IF @DemoProposalId IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM crm.ProposalEmployee pe
        WHERE pe.ProposalId = @DemoProposalId
          AND pe.EmployeeId = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Alice Demo' ORDER BY EmployeeId DESC)
          AND pe.Active = 1
    )
    BEGIN
        INSERT INTO crm.ProposalEmployee
        (
            ProposalId,
            EmployeeId,
            CostSnapshot,
            MarginPercentApplied,
            SellPriceSnapshot,
            Active,
            CreatedAt,
            UpdatedAt
        )
        VALUES
        (
            @DemoProposalId,
            (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Alice Demo' ORDER BY EmployeeId DESC),
            ISNULL(@AliceCost, 0),
            20.0000,
            ISNULL(@AliceCost, 0) * 1.20,
            1,
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        );
    END

    IF NOT EXISTS (
        SELECT 1
        FROM crm.ProposalEmployee pe
        WHERE pe.ProposalId = @DemoProposalId
          AND pe.EmployeeId = (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Bruno Demo' ORDER BY EmployeeId DESC)
          AND pe.Active = 1
    )
    BEGIN
        INSERT INTO crm.ProposalEmployee
        (
            ProposalId,
            EmployeeId,
            CostSnapshot,
            MarginPercentApplied,
            SellPriceSnapshot,
            Active,
            CreatedAt,
            UpdatedAt
        )
        VALUES
        (
            @DemoProposalId,
            (SELECT TOP 1 EmployeeId FROM hr.Employee WHERE FullName = 'Bruno Demo' ORDER BY EmployeeId DESC),
            ISNULL(@BrunoCost, 0),
            20.0000,
            ISNULL(@BrunoCost, 0) * 1.20,
            1,
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        );
    END

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
    WHERE p.ProposalId = @DemoProposalId;
END
GO
