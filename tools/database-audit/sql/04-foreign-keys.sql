-- 04-foreign-keys.sql
-- Foreign key definitions and orphan counts (IDs only in orphan counts).
SET NOCOUNT ON;

SELECT
    fk.name AS fk_name,
    OBJECT_SCHEMA_NAME(fk.parent_object_id) AS parent_schema,
    OBJECT_NAME(fk.parent_object_id) AS parent_table,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS parent_column,
    OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS referenced_schema,
    OBJECT_NAME(fk.referenced_object_id) AS referenced_table,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS referenced_column,
    fk.delete_referential_action_desc,
    fk.update_referential_action_desc
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
ORDER BY parent_table, fk.name;

-- Orphan checks for known application FKs (expected from EF model)
IF OBJECT_ID('dbo.Runners', 'U') IS NOT NULL AND OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    SELECT 'orphan_Runners_UserId' AS check_name, COUNT(*) AS orphan_count
    FROM dbo.Runners r
    LEFT JOIN dbo.Users u ON r.UserId = u.Id
    WHERE u.Id IS NULL;

IF OBJECT_ID('dbo.Cases', 'U') IS NOT NULL AND OBJECT_ID('dbo.Runners', 'U') IS NOT NULL
    SELECT 'orphan_Cases_RunnerId' AS check_name, COUNT(*) AS orphan_count
    FROM dbo.Cases c
    LEFT JOIN dbo.Runners r ON c.RunnerId = r.Id
    WHERE r.Id IS NULL;

IF OBJECT_ID('dbo.Cases', 'U') IS NOT NULL AND OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    SELECT 'orphan_Cases_ReportedByUserId' AS check_name, COUNT(*) AS orphan_count
    FROM dbo.Cases c
    LEFT JOIN dbo.Users u ON c.ReportedByUserId = u.Id
    WHERE c.ReportedByUserId IS NOT NULL AND u.Id IS NULL;

IF OBJECT_ID('dbo.Devices', 'U') IS NOT NULL AND OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    SELECT 'orphan_Devices_UserId' AS check_name, COUNT(*) AS orphan_count
    FROM dbo.Devices d
    LEFT JOIN dbo.Users u ON d.UserId = u.Id
    WHERE u.Id IS NULL;

IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL AND OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    SELECT 'orphan_Notifications_UserId' AS check_name, COUNT(*) AS orphan_count
    FROM dbo.Notifications n
    LEFT JOIN dbo.Users u ON n.UserId = u.Id
    WHERE u.Id IS NULL;

IF OBJECT_ID('dbo.TopicSubscriptions', 'U') IS NOT NULL AND OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    SELECT 'orphan_TopicSubscriptions_UserId' AS check_name, COUNT(*) AS orphan_count
    FROM dbo.TopicSubscriptions t
    LEFT JOIN dbo.Users u ON t.UserId = u.Id
    WHERE u.Id IS NULL;
