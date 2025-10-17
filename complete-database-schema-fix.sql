-- Complete Database Schema Fix for 241 Runners Awareness
-- This script ensures the Users table has all required columns for the application

PRINT 'Starting database schema fix...'

-- 1. Add AdditionalRoles column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AdditionalRoles')
BEGIN
    ALTER TABLE Users ADD AdditionalRoles NVARCHAR(200) NULL;
    PRINT '✅ Added AdditionalRoles column'
END
ELSE
BEGIN
    PRINT '✅ AdditionalRoles column already exists'
END

-- 2. Add any missing OAuth-related columns
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AuthProvider')
BEGIN
    ALTER TABLE Users ADD AuthProvider NVARCHAR(50) NULL;
    PRINT '✅ Added AuthProvider column'
END
ELSE
BEGIN
    PRINT '✅ AuthProvider column already exists'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderUserId')
BEGIN
    ALTER TABLE Users ADD ProviderUserId NVARCHAR(255) NULL;
    PRINT '✅ Added ProviderUserId column'
END
ELSE
BEGIN
    PRINT '✅ ProviderUserId column already exists'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderAccessToken')
BEGIN
    ALTER TABLE Users ADD ProviderAccessToken NVARCHAR(500) NULL;
    PRINT '✅ Added ProviderAccessToken column'
END
ELSE
BEGIN
    PRINT '✅ ProviderAccessToken column already exists'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderRefreshToken')
BEGIN
    ALTER TABLE Users ADD ProviderRefreshToken NVARCHAR(500) NULL;
    PRINT '✅ Added ProviderRefreshToken column'
END
ELSE
BEGIN
    PRINT '✅ ProviderRefreshToken column already exists'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderTokenExpires')
BEGIN
    ALTER TABLE Users ADD ProviderTokenExpires DATETIME2 NULL;
    PRINT '✅ Added ProviderTokenExpires column'
END
ELSE
BEGIN
    PRINT '✅ ProviderTokenExpires column already exists'
END

-- 3. Update existing admin users to have proper AdditionalRoles
UPDATE Users 
SET AdditionalRoles = NULL 
WHERE Role = 'admin';

PRINT '✅ Updated existing admin users'

-- 4. Verify all columns exist
PRINT ''
PRINT '=== Current Users table structure ==='
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;

-- 5. Verify all admin users are properly configured
PRINT ''
PRINT '=== Admin Users Status ==='
SELECT 
    Id, 
    Email, 
    FirstName, 
    LastName, 
    Role, 
    AdditionalRoles,
    IsActive,
    IsEmailVerified,
    IsPhoneVerified
FROM Users
WHERE Role = 'admin'
ORDER BY FirstName, LastName;

-- 6. Check total user count
PRINT ''
PRINT '=== Database Summary ==='
SELECT 
    COUNT(*) as TotalUsers,
    COUNT(CASE WHEN Role = 'admin' THEN 1 END) as AdminUsers,
    COUNT(CASE WHEN Role = 'user' THEN 1 END) as RegularUsers,
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveUsers
FROM Users;

PRINT ''
PRINT '🎉 Database schema fix completed successfully!'
PRINT 'The application should now work without 500 errors.'
