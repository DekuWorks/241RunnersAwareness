-- Fix Database Schema - Add Missing AdditionalRoles Column
-- Copy and paste this script into Azure SQL Query Editor

-- Check if AdditionalRoles column exists, if not add it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AdditionalRoles')
BEGIN
    ALTER TABLE Users ADD AdditionalRoles NVARCHAR(200) NULL;
    PRINT 'Added AdditionalRoles column to Users table'
END
ELSE
BEGIN
    PRINT 'AdditionalRoles column already exists in Users table'
END

-- Verify the column was added
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AdditionalRoles';

-- Update existing admin users to have proper AdditionalRoles (NULL for now)
UPDATE Users 
SET AdditionalRoles = NULL 
WHERE Role = 'admin' AND AdditionalRoles IS NULL;

-- Verify all admin users are properly configured
SELECT 
    Id, 
    Email, 
    FirstName, 
    LastName, 
    Role, 
    AdditionalRoles,
    IsActive
FROM Users
WHERE Role = 'admin'
ORDER BY FirstName, LastName;

PRINT 'Database schema updated successfully!'
