-- Add missing columns to Users table for proper application functionality
-- This script adds the AdditionalRoles column that the application expects

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
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AdditionalRoles';

-- Update existing admin users to have proper AdditionalRoles (empty for now)
UPDATE Users 
SET AdditionalRoles = NULL 
WHERE Role = 'admin' AND AdditionalRoles IS NULL;

PRINT 'Database schema updated successfully!'
