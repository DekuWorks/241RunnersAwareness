-- 07-ef-migrations.sql
SET NOCOUNT ON;

IF OBJECT_ID('dbo.__EFMigrationsHistory', 'U') IS NOT NULL
BEGIN
    SELECT MigrationId, ProductVersion
    FROM dbo.__EFMigrationsHistory
    ORDER BY MigrationId;
END
ELSE
BEGIN
    SELECT 'dbo.__EFMigrationsHistory not found' AS status;
END

-- Compare to expected migrations in repo (manual):
-- 20250908143434_InitialCreate through 20260309192215_AddRunnerShowOnMapAndCoordinates
