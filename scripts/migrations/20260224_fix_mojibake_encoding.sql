USE Apeiron_ONE;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER FUNCTION dbo.FixMojibake (@input NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @r NVARCHAR(MAX) = @input;
    IF @r IS NULL RETURN NULL;

    -- Common UTF-8 interpreted as ANSI sequences
    SET @r = REPLACE(@r, N'Ã¡', N'á');
    SET @r = REPLACE(@r, N'Ã ', N'à');
    SET @r = REPLACE(@r, N'Ã¢', N'â');
    SET @r = REPLACE(@r, N'Ã£', N'ã');
    SET @r = REPLACE(@r, N'Ã¤', N'ä');
    SET @r = REPLACE(@r, N'Ã©', N'é');
    SET @r = REPLACE(@r, N'Ã¨', N'è');
    SET @r = REPLACE(@r, N'Ãª', N'ê');
    SET @r = REPLACE(@r, N'Ã«', N'ë');
    SET @r = REPLACE(@r, N'Ã­', N'í');
    SET @r = REPLACE(@r, N'Ã¬', N'ì');
    SET @r = REPLACE(@r, N'Ã®', N'î');
    SET @r = REPLACE(@r, N'Ã¯', N'ï');
    SET @r = REPLACE(@r, N'Ã³', N'ó');
    SET @r = REPLACE(@r, N'Ã²', N'ò');
    SET @r = REPLACE(@r, N'Ã´', N'ô');
    SET @r = REPLACE(@r, N'Ãµ', N'õ');
    SET @r = REPLACE(@r, N'Ã¶', N'ö');
    SET @r = REPLACE(@r, N'Ãº', N'ú');
    SET @r = REPLACE(@r, N'Ã¹', N'ù');
    SET @r = REPLACE(@r, N'Ã»', N'û');
    SET @r = REPLACE(@r, N'Ã¼', N'ü');
    SET @r = REPLACE(@r, N'Ã§', N'ç');
    SET @r = REPLACE(@r, N'Ã±', N'ñ');

    SET @r = REPLACE(@r, N'Ã', N'Á');
    SET @r = REPLACE(@r, N'Ã€', N'À');
    SET @r = REPLACE(@r, N'Ã‚', N'Â');
    SET @r = REPLACE(@r, N'Ãƒ', N'Ã');
    SET @r = REPLACE(@r, N'Ã„', N'Ä');
    SET @r = REPLACE(@r, N'Ã‰', N'É');
    SET @r = REPLACE(@r, N'Ãˆ', N'È');
    SET @r = REPLACE(@r, N'ÃŠ', N'Ê');
    SET @r = REPLACE(@r, N'Ã‹', N'Ë');
    SET @r = REPLACE(@r, N'Ã', N'Í');
    SET @r = REPLACE(@r, N'ÃŒ', N'Ì');
    SET @r = REPLACE(@r, N'ÃŽ', N'Î');
    SET @r = REPLACE(@r, N'Ã', N'Ï');
    SET @r = REPLACE(@r, N'Ã“', N'Ó');
    SET @r = REPLACE(@r, N'Ã’', N'Ò');
    SET @r = REPLACE(@r, N'Ã”', N'Ô');
    SET @r = REPLACE(@r, N'Ã•', N'Õ');
    SET @r = REPLACE(@r, N'Ã–', N'Ö');
    SET @r = REPLACE(@r, N'Ãš', N'Ú');
    SET @r = REPLACE(@r, N'Ã™', N'Ù');
    SET @r = REPLACE(@r, N'Ã›', N'Û');
    SET @r = REPLACE(@r, N'Ãœ', N'Ü');
    SET @r = REPLACE(@r, N'Ã‡', N'Ç');
    SET @r = REPLACE(@r, N'Ã‘', N'Ñ');

    -- Punctuation artifacts
    SET @r = REPLACE(@r, N'â€“', N'–');
    SET @r = REPLACE(@r, N'â€”', N'—');
    SET @r = REPLACE(@r, N'â€œ', N'“');
    SET @r = REPLACE(@r, N'â€', N'”');
    SET @r = REPLACE(@r, N'â€™', N'’');
    SET @r = REPLACE(@r, N'â€˜', N'‘');
    SET @r = REPLACE(@r, N'â€¢', N'•');

    -- Stray character commonly left by mojibake
    SET @r = REPLACE(@r, N'Â', N'');

    RETURN @r;
END;
GO

DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql = @sql + N'
UPDATE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N'
SET ' + QUOTENAME(c.name) + N' = dbo.FixMojibake(' + QUOTENAME(c.name) + N')
WHERE ' + QUOTENAME(c.name) + N' IS NOT NULL
  AND (' + QUOTENAME(c.name) + N' LIKE N''%Ã%'' OR ' + QUOTENAME(c.name) + N' LIKE N''%Â%'' OR ' + QUOTENAME(c.name) + N' LIKE N''%â%'' );
'
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE s.name IN ('crm', 'hr')
  AND ty.name IN ('varchar', 'nvarchar', 'char', 'nchar', 'text', 'ntext');

EXEC sp_executesql @sql;
GO

SELECT
    (SELECT COUNT(1) FROM crm.Opportunity WHERE OpportunityName LIKE N'%Ã%' OR Description LIKE N'%Ã%') AS RemainingOpportunityMojibake,
    (SELECT COUNT(1) FROM crm.Proposal WHERE Title LIKE N'%Ã%' OR ObjectiveHtml LIKE N'%Ã%') AS RemainingProposalMojibake,
    (SELECT COUNT(1) FROM crm.Client WHERE ClientName LIKE N'%Ã%' OR LegalName LIKE N'%Ã%') AS RemainingClientMojibake;
GO
