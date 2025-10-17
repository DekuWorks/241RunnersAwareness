-- Complete Database Schema Fix for 241 Runners Awareness
-- This script adds all missing columns to fix API functionality

PRINT 'Starting comprehensive database schema fix...'

-- 1. Fix Runners table - Add missing columns
PRINT 'Fixing Runners table...'

-- Add missing columns to Runners table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'EyeColor')
BEGIN
    ALTER TABLE Runners ADD EyeColor NVARCHAR(50) NULL;
    PRINT '✅ Added EyeColor column to Runners table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'Height')
BEGIN
    ALTER TABLE Runners ADD Height NVARCHAR(20) NULL;
    PRINT '✅ Added Height column to Runners table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'Weight')
BEGIN
    ALTER TABLE Runners ADD Weight NVARCHAR(20) NULL;
    PRINT '✅ Added Weight column to Runners table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'Status')
BEGIN
    ALTER TABLE Runners ADD Status NVARCHAR(50) NULL DEFAULT 'active';
    PRINT '✅ Added Status column to Runners table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'LastPhotoUpdate')
BEGIN
    ALTER TABLE Runners ADD LastPhotoUpdate DATETIME2 NULL;
    PRINT '✅ Added LastPhotoUpdate column to Runners table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'NextPhotoReminder')
BEGIN
    ALTER TABLE Runners ADD NextPhotoReminder DATETIME2 NULL;
    PRINT '✅ Added NextPhotoReminder column to Runners table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'PhotoUpdateReminderCount')
BEGIN
    ALTER TABLE Runners ADD PhotoUpdateReminderCount INT DEFAULT 0;
    PRINT '✅ Added PhotoUpdateReminderCount column to Runners table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Runners' AND COLUMN_NAME = 'PhotoUpdateReminderSent')
BEGIN
    ALTER TABLE Runners ADD PhotoUpdateReminderSent BIT DEFAULT 0;
    PRINT '✅ Added PhotoUpdateReminderSent column to Runners table'
END

-- 2. Create Cases table if it doesn't exist
PRINT 'Creating Cases table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Cases' AND xtype='U')
BEGIN
    CREATE TABLE Cases (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CaseNumber NVARCHAR(100) NOT NULL UNIQUE,
        Title NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        Status NVARCHAR(50) NOT NULL DEFAULT 'open',
        Priority NVARCHAR(20) NOT NULL DEFAULT 'medium',
        CreatedBy INT NOT NULL,
        AssignedTo INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        ClosedAt DATETIME2 NULL,
        Location NVARCHAR(255),
        Latitude DECIMAL(10, 8) NULL,
        Longitude DECIMAL(11, 8) NULL,
        Tags NVARCHAR(500),
        IsPublic BIT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,
        Notes NVARCHAR(MAX),
        ContactInfo NVARCHAR(500),
        LastSeenDate DATETIME2 NULL,
        LastSeenLocation NVARCHAR(255),
        AgeAtDisappearance INT NULL,
        Gender NVARCHAR(20),
        Race NVARCHAR(50),
        HairColor NVARCHAR(50),
        EyeColor NVARCHAR(50),
        Height NVARCHAR(20),
        Weight NVARCHAR(20),
        ClothingDescription NVARCHAR(MAX),
        PhysicalDescription NVARCHAR(MAX),
        MedicalConditions NVARCHAR(MAX),
        OtherIdentifyingMarks NVARCHAR(MAX),
        Photos NVARCHAR(MAX), -- JSON array of photo URLs
        Documents NVARCHAR(MAX), -- JSON array of document URLs
        RelatedCases NVARCHAR(MAX), -- JSON array of related case IDs
        InvestigationNotes NVARCHAR(MAX),
        LawEnforcementContact NVARCHAR(255),
        CaseOfficer NVARCHAR(255),
        CaseOfficerPhone NVARCHAR(20),
        CaseOfficerEmail NVARCHAR(255),
        Agency NVARCHAR(255),
        ReportNumber NVARCHAR(100),
        DateReported DATETIME2 NULL,
        DateLastModified DATETIME2 NULL DEFAULT GETUTCDATE(),
        ModifiedBy INT NULL,
        Version INT DEFAULT 1,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy INT NULL
    );
    
    -- Add foreign key constraints
    ALTER TABLE Cases ADD CONSTRAINT FK_Cases_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
    ALTER TABLE Cases ADD CONSTRAINT FK_Cases_AssignedTo FOREIGN KEY (AssignedTo) REFERENCES Users(Id);
    ALTER TABLE Cases ADD CONSTRAINT FK_Cases_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
    ALTER TABLE Cases ADD CONSTRAINT FK_Cases_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id);
    
    -- Create indexes
    CREATE INDEX IX_Cases_Status ON Cases(Status);
    CREATE INDEX IX_Cases_Priority ON Cases(Priority);
    CREATE INDEX IX_Cases_CreatedBy ON Cases(CreatedBy);
    CREATE INDEX IX_Cases_AssignedTo ON Cases(AssignedTo);
    CREATE INDEX IX_Cases_IsPublic ON Cases(IsPublic);
    CREATE INDEX IX_Cases_IsActive ON Cases(IsActive);
    CREATE INDEX IX_Cases_CreatedAt ON Cases(CreatedAt);
    CREATE INDEX IX_Cases_Location ON Cases(Location);
    
    PRINT '✅ Cases table created successfully'
END
ELSE
BEGIN
    PRINT '✅ Cases table already exists'
END

-- 3. Create Notifications table if it doesn't exist
PRINT 'Creating Notifications table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
BEGIN
    CREATE TABLE Notifications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Title NVARCHAR(255) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        Type NVARCHAR(50) NOT NULL DEFAULT 'info',
        Priority NVARCHAR(20) NOT NULL DEFAULT 'normal',
        IsRead BIT NOT NULL DEFAULT 0,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ReadAt DATETIME2 NULL,
        ExpiresAt DATETIME2 NULL,
        ActionUrl NVARCHAR(500),
        ActionText NVARCHAR(100),
        Metadata NVARCHAR(MAX), -- JSON data
        CreatedBy INT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
    );
    
    CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
    CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
    CREATE INDEX IX_Notifications_Type ON Notifications(Type);
    CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt);
    
    PRINT '✅ Notifications table created successfully'
END
ELSE
BEGIN
    PRINT '✅ Notifications table already exists'
END

-- 4. Create Topics table if it doesn't exist
PRINT 'Creating Topics table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Topics' AND xtype='U')
BEGIN
    CREATE TABLE Topics (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy INT NULL,
        FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
    );
    
    PRINT '✅ Topics table created successfully'
END
ELSE
BEGIN
    PRINT '✅ Topics table already exists'
END

-- 5. Create TopicSubscriptions table if it doesn't exist
PRINT 'Creating TopicSubscriptions table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TopicSubscriptions' AND xtype='U')
BEGIN
    CREATE TABLE TopicSubscriptions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        TopicId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        SubscribedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (TopicId) REFERENCES Topics(Id),
        UNIQUE(UserId, TopicId)
    );
    
    CREATE INDEX IX_TopicSubscriptions_UserId ON TopicSubscriptions(UserId);
    CREATE INDEX IX_TopicSubscriptions_TopicId ON TopicSubscriptions(TopicId);
    
    PRINT '✅ TopicSubscriptions table created successfully'
END
ELSE
BEGIN
    PRINT '✅ TopicSubscriptions table already exists'
END

-- 6. Create Devices table if it doesn't exist
PRINT 'Creating Devices table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Devices' AND xtype='U')
BEGIN
    CREATE TABLE Devices (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        DeviceId NVARCHAR(255) NOT NULL,
        DeviceName NVARCHAR(255),
        DeviceType NVARCHAR(50) NOT NULL, -- 'mobile', 'web', 'desktop'
        Platform NVARCHAR(50), -- 'ios', 'android', 'windows', 'mac', 'linux'
        PushToken NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        LastSeenAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        UNIQUE(UserId, DeviceId)
    );
    
    CREATE INDEX IX_Devices_UserId ON Devices(UserId);
    CREATE INDEX IX_Devices_DeviceType ON Devices(DeviceType);
    CREATE INDEX IX_Devices_IsActive ON Devices(IsActive);
    
    PRINT '✅ Devices table created successfully'
END
ELSE
BEGIN
    PRINT '✅ Devices table already exists'
END

-- 7. Create Reports table if it doesn't exist
PRINT 'Creating Reports table...'
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reports' AND xtype='U')
BEGIN
    CREATE TABLE Reports (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CaseId INT NULL,
        ReporterId INT NOT NULL,
        Title NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        Type NVARCHAR(50) NOT NULL, -- 'sighting', 'tip', 'information', 'other'
        Priority NVARCHAR(20) NOT NULL DEFAULT 'medium',
        Status NVARCHAR(50) NOT NULL DEFAULT 'new',
        Location NVARCHAR(255),
        Latitude DECIMAL(10, 8) NULL,
        Longitude DECIMAL(11, 8) NULL,
        ContactInfo NVARCHAR(500),
        IsAnonymous BIT NOT NULL DEFAULT 0,
        IsPublic BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        AssignedTo INT NULL,
        AssignedAt DATETIME2 NULL,
        ResolvedAt DATETIME2 NULL,
        Resolution NVARCHAR(MAX),
        Notes NVARCHAR(MAX),
        Attachments NVARCHAR(MAX), -- JSON array of file URLs
        FOREIGN KEY (CaseId) REFERENCES Cases(Id),
        FOREIGN KEY (ReporterId) REFERENCES Users(Id),
        FOREIGN KEY (AssignedTo) REFERENCES Users(Id)
    );
    
    CREATE INDEX IX_Reports_CaseId ON Reports(CaseId);
    CREATE INDEX IX_Reports_ReporterId ON Reports(ReporterId);
    CREATE INDEX IX_Reports_Status ON Reports(Status);
    CREATE INDEX IX_Reports_Type ON Reports(Type);
    CREATE INDEX IX_Reports_CreatedAt ON Reports(CreatedAt);
    
    PRINT '✅ Reports table created successfully'
END
ELSE
BEGIN
    PRINT '✅ Reports table already exists'
END

-- 8. Add missing columns to Users table for OAuth
PRINT 'Adding OAuth columns to Users table...'

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AuthProvider')
BEGIN
    ALTER TABLE Users ADD AuthProvider NVARCHAR(50) NULL;
    PRINT '✅ Added AuthProvider column to Users table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderUserId')
BEGIN
    ALTER TABLE Users ADD ProviderUserId NVARCHAR(255) NULL;
    PRINT '✅ Added ProviderUserId column to Users table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderAccessToken')
BEGIN
    ALTER TABLE Users ADD ProviderAccessToken NVARCHAR(500) NULL;
    PRINT '✅ Added ProviderAccessToken column to Users table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderRefreshToken')
BEGIN
    ALTER TABLE Users ADD ProviderRefreshToken NVARCHAR(500) NULL;
    PRINT '✅ Added ProviderRefreshToken column to Users table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderTokenExpires')
BEGIN
    ALTER TABLE Users ADD ProviderTokenExpires DATETIME2 NULL;
    PRINT '✅ Added ProviderTokenExpires column to Users table'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AdditionalRoles')
BEGIN
    ALTER TABLE Users ADD AdditionalRoles NVARCHAR(200) NULL;
    PRINT '✅ Added AdditionalRoles column to Users table'
END

-- 9. Insert default topics
PRINT 'Inserting default topics...'
IF NOT EXISTS (SELECT * FROM Topics WHERE Name = 'case-updates')
BEGIN
    INSERT INTO Topics (Name, Description, CreatedBy) VALUES 
    ('case-updates', 'Updates about missing persons cases', 1),
    ('system-alerts', 'System-wide alerts and notifications', 1),
    ('admin-announcements', 'Administrative announcements', 1),
    ('new-cases', 'Notifications about new cases', 1),
    ('case-resolved', 'Notifications when cases are resolved', 1);
    PRINT '✅ Default topics inserted'
END

-- 10. Verify all tables and columns exist
PRINT 'Verifying database schema...'

-- Check Runners table columns
SELECT 
    'Runners' as TableName,
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Runners' 
  AND COLUMN_NAME IN ('EyeColor', 'Height', 'Weight', 'Status', 'LastPhotoUpdate', 'NextPhotoReminder', 'PhotoUpdateReminderCount', 'PhotoUpdateReminderSent')
ORDER BY COLUMN_NAME;

-- Check Cases table
SELECT 
    'Cases' as TableName,
    COUNT(*) as ColumnCount
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Cases';

-- Check other tables
SELECT 
    'Notifications' as TableName,
    COUNT(*) as ColumnCount
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Notifications';

SELECT 
    'Topics' as TableName,
    COUNT(*) as ColumnCount
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Topics';

SELECT 
    'Devices' as TableName,
    COUNT(*) as ColumnCount
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Devices';

SELECT 
    'Reports' as TableName,
    COUNT(*) as ColumnCount
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Reports';

PRINT '🎉 Database schema fix completed successfully!'
PRINT 'All required tables and columns have been created/updated.'
PRINT 'The API should now work properly with all endpoints.'
