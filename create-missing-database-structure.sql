-- Comprehensive Database Structure Creation
-- This script creates all missing tables and columns needed for the API

PRINT 'Starting comprehensive database structure creation...'

-- 1. Add missing columns to existing Users table
PRINT 'Adding missing columns to Users table...'

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AuthProvider')
    ALTER TABLE Users ADD AuthProvider NVARCHAR(50) NULL;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderUserId')
    ALTER TABLE Users ADD ProviderUserId NVARCHAR(255) NULL;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderAccessToken')
    ALTER TABLE Users ADD ProviderAccessToken NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderRefreshToken')
    ALTER TABLE Users ADD ProviderRefreshToken NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderTokenExpires')
    ALTER TABLE Users ADD ProviderTokenExpires DATETIME2 NULL;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AdditionalRoles')
    ALTER TABLE Users ADD AdditionalRoles NVARCHAR(200) NULL;

-- 2. Create Cases table if it doesn't exist
PRINT 'Creating Cases table...'
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
    PRINT 'Cases table created successfully!'
END

-- 3. Create Runners table if it doesn't exist
PRINT 'Creating Runners table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Runners' AND xtype='U')
BEGIN
    CREATE TABLE Runners (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        FullName AS (FirstName + ' ' + LastName),
        DateOfBirth DATE,
        Gender NVARCHAR(20),
        EyeColor NVARCHAR(50),
        HairColor NVARCHAR(50),
        Height NVARCHAR(50),
        Weight NVARCHAR(50),
        Build NVARCHAR(50),
        DistinguishingMarks NVARCHAR(500),
        ClothingDescription NVARCHAR(500),
        LastKnownLocation NVARCHAR(255),
        LastSeenDate DATETIME2,
        LastSeenTime NVARCHAR(20),
        ContactInfo NVARCHAR(255),
        EmergencyContact NVARCHAR(255),
        MedicalConditions NVARCHAR(500),
        Medications NVARCHAR(500),
        Allergies NVARCHAR(500),
        SpecialNeeds NVARCHAR(500),
        Status NVARCHAR(50) DEFAULT 'Missing',
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        LastPhotoUpdate DATETIME2,
        NextPhotoReminder DATETIME2,
        PhotoUpdateReminderCount INT DEFAULT 0,
        PhotoUpdateReminderSent BIT DEFAULT 0,
        ProfileImageUrl NVARCHAR(500),
        AdditionalImages NVARCHAR(MAX),
        Notes NVARCHAR(MAX),
        FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    PRINT 'Runners table created successfully!'
END

-- 4. Add missing columns to Runners table if it exists
PRINT 'Adding missing columns to Runners table...'
IF EXISTS (SELECT * FROM sysobjects WHERE name='Runners' AND xtype='U')
BEGIN
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'EyeColor')
        ALTER TABLE Runners ADD EyeColor NVARCHAR(50) NULL;
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'Height')
        ALTER TABLE Runners ADD Height NVARCHAR(50) NULL;
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'Weight')
        ALTER TABLE Runners ADD Weight NVARCHAR(50) NULL;
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'LastPhotoUpdate')
        ALTER TABLE Runners ADD LastPhotoUpdate DATETIME2 NULL;
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'NextPhotoReminder')
        ALTER TABLE Runners ADD NextPhotoReminder DATETIME2 NULL;
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'PhotoUpdateReminderCount')
        ALTER TABLE Runners ADD PhotoUpdateReminderCount INT DEFAULT 0;
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'PhotoUpdateReminderSent')
        ALTER TABLE Runners ADD PhotoUpdateReminderSent BIT DEFAULT 0;
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'Status')
        ALTER TABLE Runners ADD Status NVARCHAR(50) NULL;
END

-- 5. Create Notifications table
PRINT 'Creating Notifications table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
BEGIN
    CREATE TABLE Notifications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Title NVARCHAR(255) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        Priority NVARCHAR(20) DEFAULT 'Normal',
        IsRead BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ReadAt DATETIME2,
        ExpiresAt DATETIME2,
        ActionUrl NVARCHAR(500),
        Metadata NVARCHAR(MAX),
        FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    PRINT 'Notifications table created successfully!'
END

-- 6. Create Topics table
PRINT 'Creating Topics table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Topics' AND xtype='U')
BEGIN
    CREATE TABLE Topics (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        Category NVARCHAR(50),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2
    );
    PRINT 'Topics table created successfully!'
END

-- 7. Create Devices table
PRINT 'Creating Devices table...'
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
    PRINT 'Devices table created successfully!'
END

-- 8. Create Reports table
PRINT 'Creating Reports table...'
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
    PRINT 'Reports table created successfully!'
END

-- 9. Create FileUploads table
PRINT 'Creating FileUploads table...'
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
    PRINT 'FileUploads table created successfully!'
END

-- 10. Insert default topics
PRINT 'Inserting default topics...'
IF NOT EXISTS (SELECT * FROM Topics WHERE Name = 'Missing Person')
    INSERT INTO Topics (Name, Description, Category) VALUES ('Missing Person', 'Reports about missing individuals', 'General');

IF NOT EXISTS (SELECT * FROM Topics WHERE Name = 'Emergency')
    INSERT INTO Topics (Name, Description, Category) VALUES ('Emergency', 'Urgent situations requiring immediate attention', 'Emergency');

IF NOT EXISTS (SELECT * FROM Topics WHERE Name = 'Safety Alert')
    INSERT INTO Topics (Name, Description, Category) VALUES ('Safety Alert', 'Safety-related notifications and alerts', 'Safety');

-- 11. Verify all tables exist
PRINT 'Verifying database structure...'
SELECT 
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Users', 'Cases', 'Runners', 'Notifications', 'Topics', 'Devices', 'Reports', 'FileUploads')
ORDER BY TABLE_NAME;

PRINT 'Comprehensive database structure creation completed successfully!'
