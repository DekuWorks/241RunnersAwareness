-- 03-data-quality.sql
-- Statistical summaries only. No raw PII values.
SET NOCOUNT ON;

-- Users: verification and role distribution (no email/phone values)
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
BEGIN
    SELECT 'Users' AS table_name, COUNT(*) AS total_rows FROM dbo.Users;
    SELECT 'Users.Role' AS metric, Role AS value, COUNT(*) AS cnt FROM dbo.Users GROUP BY Role ORDER BY cnt DESC;
    SELECT 'Users.IsActive' AS metric, CAST(IsActive AS varchar(5)) AS value, COUNT(*) AS cnt FROM dbo.Users GROUP BY IsActive;
    SELECT 'Users.IsEmailVerified' AS metric, CAST(IsEmailVerified AS varchar(5)) AS value, COUNT(*) AS cnt FROM dbo.Users GROUP BY IsEmailVerified;
    SELECT 'Users.AuthProvider' AS metric, ISNULL(AuthProvider, '(null)') AS value, COUNT(*) AS cnt FROM dbo.Users GROUP BY AuthProvider ORDER BY cnt DESC;
    SELECT 'duplicate_email_count' AS metric, COUNT(*) - COUNT(DISTINCT Email) AS value FROM dbo.Users;
    SELECT 'null_password_hash_oauth_users' AS metric, COUNT(*) AS value FROM dbo.Users WHERE PasswordHash IS NULL OR PasswordHash = '';
END

-- Runners: status distribution (no names/medical text)
IF OBJECT_ID('dbo.Runners', 'U') IS NOT NULL
BEGIN
    SELECT 'Runners' AS table_name, COUNT(*) AS total_rows FROM dbo.Runners;
    SELECT 'Runners.Status' AS metric, Status AS value, COUNT(*) AS cnt FROM dbo.Runners GROUP BY Status ORDER BY cnt DESC;
    SELECT 'Runners.ShowOnMap' AS metric, CAST(ShowOnMap AS varchar(5)) AS value, COUNT(*) AS cnt FROM dbo.Runners GROUP BY ShowOnMap;
    SELECT 'runners_with_map_coordinates' AS metric, COUNT(*) AS value FROM dbo.Runners WHERE MapLatitude IS NOT NULL AND MapLongitude IS NOT NULL;
    SELECT 'runners_with_profile_image_url' AS metric, COUNT(*) AS value FROM dbo.Runners WHERE ProfileImageUrl IS NOT NULL AND ProfileImageUrl <> '';
END

-- Cases: status and visibility (no descriptions)
IF OBJECT_ID('dbo.Cases', 'U') IS NOT NULL
BEGIN
    SELECT 'Cases' AS table_name, COUNT(*) AS total_rows FROM dbo.Cases;
    SELECT 'Cases.Status' AS metric, Status AS value, COUNT(*) AS cnt FROM dbo.Cases GROUP BY Status ORDER BY cnt DESC;
    SELECT 'Cases.IsPublic' AS metric, CAST(IsPublic AS varchar(5)) AS value, COUNT(*) AS cnt FROM dbo.Cases GROUP BY IsPublic;
    SELECT 'cases_with_additional_information_json' AS metric, COUNT(*) AS value FROM dbo.Cases WHERE AdditionalInformation IS NOT NULL AND LEN(AdditionalInformation) > 2;
END

-- Devices, Notifications, Topics
IF OBJECT_ID('dbo.Devices', 'U') IS NOT NULL
    SELECT 'Devices' AS table_name, COUNT(*) AS total_rows FROM dbo.Devices;
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL
    SELECT 'Notifications' AS table_name, COUNT(*) AS total_rows FROM dbo.Notifications;
IF OBJECT_ID('dbo.TopicSubscriptions', 'U') IS NOT NULL
    SELECT 'TopicSubscriptions' AS table_name, COUNT(*) AS total_rows FROM dbo.TopicSubscriptions;
IF OBJECT_ID('dbo.DataDeletionRequests', 'U') IS NOT NULL
    SELECT 'DataDeletionRequests' AS table_name, COUNT(*) AS total_rows FROM dbo.DataDeletionRequests;
IF OBJECT_ID('dbo.AccountDeletionRequests', 'U') IS NOT NULL
    SELECT 'AccountDeletionRequests' AS table_name, COUNT(*) AS total_rows FROM dbo.AccountDeletionRequests;

-- Invalid email format count (pattern only, no values)
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
BEGIN
    SELECT 'invalid_email_format_count' AS metric, COUNT(*) AS value
    FROM dbo.Users
    WHERE Email NOT LIKE '%_@_%._%' AND Email IS NOT NULL;
END

-- Timestamp ranges (no row identifiers)
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    SELECT 'Users.CreatedAt' AS column_name, MIN(CreatedAt) AS min_ts, MAX(CreatedAt) AS max_ts FROM dbo.Users;
IF OBJECT_ID('dbo.Runners', 'U') IS NOT NULL
    SELECT 'Runners.CreatedAt' AS column_name, MIN(CreatedAt) AS min_ts, MAX(CreatedAt) AS max_ts FROM dbo.Runners;
IF OBJECT_ID('dbo.Cases', 'U') IS NOT NULL
    SELECT 'Cases.CreatedAt' AS column_name, MIN(CreatedAt) AS min_ts, MAX(CreatedAt) AS max_ts FROM dbo.Cases;
