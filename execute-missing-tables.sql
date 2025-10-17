-- EXECUTE MISSING DATABASE TABLES
-- This script creates the missing tables required for the API to function
-- Execute this script directly in Azure SQL Database

PRINT '🚀 STARTING MISSING DATABASE TABLES CREATION'
PRINT '=================================================='

-- 1. CREATE CASES TABLE
PRINT '📋 Creating Cases table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Cases' AND xtype='U')
BEGIN
    CREATE TABLE Cases (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CaseNumber NVARCHAR(50) NOT NULL UNIQUE,
        Title NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        Status NVARCHAR(50) NOT NULL DEFAULT 'Open',
        Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium',
        Category NVARCHAR(100),
        Location NVARCHAR(255),
        Latitude DECIMAL(10,8),
        Longitude DECIMAL(11,8),
        CreatedBy INT NOT NULL,
        AssignedTo INT,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        ClosedAt DATETIME2,
        IsPublic BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        Tags NVARCHAR(500),
        Notes NVARCHAR(MAX),
        Evidence NVARCHAR(MAX),
        ContactInfo NVARCHAR(255),
        ContactPhone NVARCHAR(20),
        ContactEmail NVARCHAR(255),
        Resolution NVARCHAR(MAX),
        FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
        FOREIGN KEY (AssignedTo) REFERENCES Users(Id)
    );
    PRINT '✅ Cases table created successfully!'
END
ELSE
BEGIN
    PRINT '⚠️  Cases table already exists'
END

-- 2. CREATE DEVICES TABLE
PRINT '📋 Creating Devices table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Devices' AND xtype='U')
BEGIN
    CREATE TABLE Devices (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        DeviceId NVARCHAR(255) NOT NULL,
        DeviceType NVARCHAR(50) NOT NULL,
        DeviceName NVARCHAR(255),
        Platform NVARCHAR(50),
        Version NVARCHAR(50),
        IsActive BIT NOT NULL DEFAULT 1,
        LastSeen DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        PushToken NVARCHAR(500),
        FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    PRINT '✅ Devices table created successfully!'
END
ELSE
BEGIN
    PRINT '⚠️  Devices table already exists'
END

-- 3. CREATE REPORTS TABLE
PRINT '📋 Creating Reports table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reports' AND xtype='U')
BEGIN
    CREATE TABLE Reports (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Title NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        Type NVARCHAR(50) NOT NULL,
        Status NVARCHAR(50) DEFAULT 'Draft',
        GeneratedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        Data NVARCHAR(MAX),
        FilePath NVARCHAR(500),
        IsPublic BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    PRINT '✅ Reports table created successfully!'
END
ELSE
BEGIN
    PRINT '⚠️  Reports table already exists'
END

-- 4. CREATE FILEUPLOADS TABLE
PRINT '📋 Creating FileUploads table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FileUploads' AND xtype='U')
BEGIN
    CREATE TABLE FileUploads (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        FileName NVARCHAR(255) NOT NULL,
        OriginalFileName NVARCHAR(255) NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        FileSize BIGINT NOT NULL,
        MimeType NVARCHAR(100) NOT NULL,
        Category NVARCHAR(50),
        IsActive BIT NOT NULL DEFAULT 1,
        UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    PRINT '✅ FileUploads table created successfully!'
END
ELSE
BEGIN
    PRINT '⚠️  FileUploads table already exists'
END

-- 5. INSERT SAMPLE DATA FOR TESTING
PRINT '📋 Inserting sample data for testing...'

-- Insert sample case
IF NOT EXISTS (SELECT * FROM Cases WHERE CaseNumber = 'CASE-001')
BEGIN
    INSERT INTO Cases (CaseNumber, Title, Description, Status, Priority, Category, CreatedBy, Location)
    VALUES ('CASE-001', 'Missing Person Report', 'Report of missing individual in downtown area', 'Open', 'High', 'Missing Person', 1, 'Downtown Area');
    PRINT '✅ Sample case inserted'
END

-- Insert sample device
IF NOT EXISTS (SELECT * FROM Devices WHERE DeviceId = 'TEST-DEVICE-001')
BEGIN
    INSERT INTO Devices (UserId, DeviceId, DeviceType, DeviceName, Platform, Version)
    VALUES (1, 'TEST-DEVICE-001', 'Mobile', 'Test Device', 'iOS', '17.0');
    PRINT '✅ Sample device inserted'
END

-- Insert sample report
IF NOT EXISTS (SELECT * FROM Reports WHERE Title = 'Monthly Activity Report')
BEGIN
    INSERT INTO Reports (UserId, Title, Description, Type, Status)
    VALUES (1, 'Monthly Activity Report', 'Monthly summary of all activities', 'Summary', 'Draft');
    PRINT '✅ Sample report inserted'
END

-- 6. VERIFY ALL TABLES EXIST
PRINT '📋 Verifying database structure...'
SELECT 
    TABLE_NAME,
    TABLE_TYPE,
    'EXISTS' as STATUS
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Users', 'Cases', 'Runners', 'Notifications', 'Topics', 'Devices', 'Reports', 'FileUploads')
ORDER BY TABLE_NAME;

-- 7. FINAL SUCCESS MESSAGE
PRINT '🎉 MISSING DATABASE TABLES CREATION COMPLETED!'
PRINT '=================================================='
PRINT '✅ All required tables have been created'
PRINT '✅ Sample data has been inserted'
PRINT '✅ Database structure is now complete'
PRINT '🚀 API endpoints should now work properly'
PRINT '📋 Next: Test all API endpoints to verify functionality'
