-- 06-sensitive-column-inventory.sql
-- Flags columns that may contain PII or sensitive data. No values selected.
SET NOCOUNT ON;

;WITH sensitive_patterns AS (
    SELECT pattern FROM (VALUES
        ('%Email%'), ('%Phone%'), ('%Address%'), ('%Password%'), ('%Hash%'),
        ('%Token%'), ('%Medical%'), ('%Medication%'), ('%Allergy%'),
        ('%Emergency%'), ('%Latitude%'), ('%Longitude%'), ('%Location%'),
        ('%Notes%'), ('%Description%'), ('%Body%'), ('%Fcm%'), ('%Provider%'),
        ('%ImageUrl%'), ('%Document%'), ('%AdditionalInformation%')
    ) AS p(pattern)
)
SELECT
    t.name AS table_name,
    c.name AS column_name,
    ty.name AS data_type,
    c.max_length,
    c.is_nullable,
    'REVIEW_FOR_MIGRATION' AS sensitivity_flag
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE s.name = 'dbo'
  AND t.is_ms_shipped = 0
  AND EXISTS (
      SELECT 1 FROM sensitive_patterns sp
      WHERE c.name LIKE sp.pattern
  )
ORDER BY t.name, c.column_id;

-- URL / blob path columns (file references)
SELECT
    t.name AS table_name,
    c.name AS column_name,
    'FILE_URL_OR_PATH' AS flag
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id
WHERE s.name = 'dbo'
  AND (c.name LIKE '%Url%' OR c.name LIKE '%Image%' OR c.name LIKE '%Document%')
ORDER BY t.name, c.name;
