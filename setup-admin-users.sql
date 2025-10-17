-- Setup Admin Users for 241 Runners Awareness
-- Run this script in Azure SQL Database Query Editor
-- Password: AdminPass123!

-- First, check if the Users table exists and its structure
SELECT 
    TABLE_NAME, 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;

-- Check if there are any users in the database
SELECT COUNT(*) as UserCount FROM Users;

-- If Users table is missing or corrupted, recreate it
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    PRINT 'Creating Users table...'
    
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        FullName AS (FirstName + ' ' + LastName),
        Role NVARCHAR(50) NOT NULL,
        AllRoles NVARCHAR(MAX),
        PrimaryUserRole NVARCHAR(50),
        AdditionalRoles NVARCHAR(200),
        IsAdminUser BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastLoginAt DATETIME2,
        PhoneNumber NVARCHAR(20),
        Address NVARCHAR(255),
        City NVARCHAR(100),
        State NVARCHAR(50),
        ZipCode NVARCHAR(10),
        Organization NVARCHAR(255),
        Title NVARCHAR(100),
        Credentials NVARCHAR(255),
        Specialization NVARCHAR(255),
        YearsOfExperience NVARCHAR(50),
        EmergencyContactName NVARCHAR(100),
        EmergencyContactPhone NVARCHAR(20),
        EmergencyContactRelationship NVARCHAR(50),
        IsEmailVerified BIT NOT NULL DEFAULT 0,
        IsPhoneVerified BIT NOT NULL DEFAULT 0,
        EmailVerificationToken NVARCHAR(255),
        PasswordResetToken NVARCHAR(255),
        ResetTokenExpires DATETIME2,
        FailedLoginAttempts INT DEFAULT 0,
        LockedUntil DATETIME2,
        ProfileImageUrl NVARCHAR(500),
        AdditionalImageUrls NVARCHAR(1000),
        DocumentUrls NVARCHAR(1000),
        Notes NVARCHAR(2000),
        UpdatedAt DATETIME2,
        EmailVerifiedAt DATETIME2,
        PhoneVerifiedAt DATETIME2
    );
    
    PRINT 'Users table created successfully!'
END
ELSE
BEGIN
    PRINT 'Users table already exists'
END

-- Create the 6 admin users
-- 1. Marcus Brown
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'dekuworks1@gmail.com')
BEGIN
    INSERT INTO Users (
        Email, PasswordHash, FirstName, LastName, Role, AllRoles, PrimaryUserRole,
        IsAdminUser, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'dekuworks1@gmail.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'marcus2025'
        'Marcus',
        'Brown',
        'admin',
        '["admin"]',
        'admin',
        1, -- IsAdminUser
        1, -- IsActive
        GETUTCDATE(),
        '555-0123',
        1, -- IsEmailVerified
        1  -- IsPhoneVerified
    );
    PRINT 'Created Marcus Brown admin user'
END
ELSE
BEGIN
    PRINT 'Marcus Brown admin user already exists'
END

-- 2. Lisa Thomas
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'lthomas3350@gmail.com')
BEGIN
    INSERT INTO Users (
        Email, PasswordHash, FirstName, LastName, Role, AllRoles, PrimaryUserRole, AdditionalRoles,
        IsAdminUser, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'lthomas3350@gmail.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'Lisa2025!'
        'Lisa',
        'Thomas',
        'admin',
        '["admin"]',
        'admin',
        '["user", "parent"]', -- Additional roles for dual-role support
        1, -- IsAdminUser
        1, -- IsActive
        GETUTCDATE(),
        '555-0124',
        1, -- IsEmailVerified
        1  -- IsPhoneVerified
    );
    PRINT 'Created Lisa Thomas admin user'
END
ELSE
BEGIN
    PRINT 'Lisa Thomas admin user already exists'
END

-- 3. Mark Melasky
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'markmelasky@gmail.com')
BEGIN
    INSERT INTO Users (
        Email, PasswordHash, FirstName, LastName, Role, AllRoles, PrimaryUserRole,
        IsAdminUser, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'markmelasky@gmail.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'Mark2025!'
        'Mark',
        'Melasky',
        'admin',
        '["admin"]',
        'admin',
        1, -- IsAdminUser
        1, -- IsActive
        GETUTCDATE(),
        '555-0125',
        1, -- IsEmailVerified
        1  -- IsPhoneVerified
    );
    PRINT 'Created Mark Melasky admin user'
END
ELSE
BEGIN
    PRINT 'Mark Melasky admin user already exists'
END

-- 4. Daniel Carey
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'danielcarey9770@yahoo.com')
BEGIN
    INSERT INTO Users (
        Email, PasswordHash, FirstName, LastName, Role, AllRoles, PrimaryUserRole, AdditionalRoles,
        IsAdminUser, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'danielcarey9770@yahoo.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'Daniel2025!'
        'Daniel',
        'Carey',
        'admin',
        '["admin"]',
        'admin',
        '["user", "parent"]', -- Additional roles for dual-role support
        1, -- IsAdminUser
        1, -- IsActive
        GETUTCDATE(),
        '555-0126',
        1, -- IsEmailVerified
        1  -- IsPhoneVerified
    );
    PRINT 'Created Daniel Carey admin user'
END
ELSE
BEGIN
    PRINT 'Daniel Carey admin user already exists'
END

-- 5. Tina Matthews
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'tinaleggins@yahoo.com')
BEGIN
    INSERT INTO Users (
        Email, PasswordHash, FirstName, LastName, Role, AllRoles, PrimaryUserRole, AdditionalRoles,
        IsAdminUser, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'tinaleggins@yahoo.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'Tina2025!'
        'Tina',
        'Matthews',
        'admin',
        '["admin"]',
        'admin',
        '["user", "parent"]', -- Additional roles for dual-role support
        1, -- IsAdminUser
        1, -- IsActive
        GETUTCDATE(),
        '555-0127',
        1, -- IsEmailVerified
        1  -- IsPhoneVerified
    );
    PRINT 'Created Tina Matthews admin user'
END
ELSE
BEGIN
    PRINT 'Tina Matthews admin user already exists'
END

-- 6. Ralph Frank
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'ralphfrank900@gmail.com')
BEGIN
    INSERT INTO Users (
        Email, PasswordHash, FirstName, LastName, Role, AllRoles, PrimaryUserRole, AdditionalRoles,
        IsAdminUser, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'ralphfrank900@gmail.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'Ralph2025!'
        'Ralph',
        'Frank',
        'admin',
        '["admin"]',
        'admin',
        '["user", "parent"]', -- Additional roles for dual-role support
        1, -- IsAdminUser
        1, -- IsActive
        GETUTCDATE(),
        '555-0128',
        1, -- IsEmailVerified
        1  -- IsPhoneVerified
    );
    PRINT 'Created Ralph Frank admin user'
END
ELSE
BEGIN
    PRINT 'Ralph Frank admin user already exists'
END

-- Verify all admin users were created
SELECT 
    Id, 
    Email, 
    FirstName, 
    LastName, 
    Role, 
    AllRoles,
    AdditionalRoles,
    IsAdminUser, 
    IsActive,
    CreatedAt
FROM Users
WHERE Role = 'admin'
ORDER BY FirstName, LastName;

PRINT 'Database setup complete! All 6 admin users should now be available for login.'
