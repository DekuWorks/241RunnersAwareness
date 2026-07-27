-- 02-row-counts.sql
-- Row counts only. No column values.
SET NOCOUNT ON;

DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql = @sql + N'
SELECT ''' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ''' AS table_name, COUNT_BIG(*) AS row_count FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + N' UNION ALL'
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.is_ms_shipped = 0 AND s.name = 'dbo';

IF LEN(@sql) > 0
BEGIN
    SET @sql = LEFT(@sql, LEN(@sql) - LEN(' UNION ALL'));
    SET @sql = @sql + N' ORDER BY table_name;';
    EXEC sp_executesql @sql;
END

-- EF migrations history (if present)
IF OBJECT_ID('dbo.__EFMigrationsHistory', 'U') IS NOT NULL
BEGIN
    SELECT 'dbo.__EFMigrationsHistory' AS table_name, COUNT_BIG(*) AS row_count FROM dbo.__EFMigrationsHistory;
END
