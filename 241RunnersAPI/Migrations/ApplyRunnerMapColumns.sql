-- AddRunnerShowOnMapAndCoordinates: add ShowOnMap, MapLatitude, MapLongitude to Runners
-- Run this in Azure SQL Query Editor (or any client with access to your database) if dotnet ef database update fails due to firewall.

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Runners') AND name = 'ShowOnMap')
BEGIN
    ALTER TABLE [Runners] ADD [ShowOnMap] bit NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Runners') AND name = 'MapLatitude')
BEGIN
    ALTER TABLE [Runners] ADD [MapLatitude] decimal(18,6) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Runners') AND name = 'MapLongitude')
BEGIN
    ALTER TABLE [Runners] ADD [MapLongitude] decimal(18,6) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260309192215_AddRunnerShowOnMapAndCoordinates')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260309192215_AddRunnerShowOnMapAndCoordinates', N'8.0.11');
END;

COMMIT;
