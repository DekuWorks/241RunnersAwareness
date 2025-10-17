-- Apply OAuth columns migration to fix login API
-- This will add the missing columns that the API expects

-- Add OAuth columns if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AuthProvider')
BEGIN
    ALTER TABLE Users ADD AuthProvider NVARCHAR(50) NULL;
    PRINT 'Added AuthProvider column'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderUserId')
BEGIN
    ALTER TABLE Users ADD ProviderUserId NVARCHAR(255) NULL;
    PRINT 'Added ProviderUserId column'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderAccessToken')
BEGIN
    ALTER TABLE Users ADD ProviderAccessToken NVARCHAR(500) NULL;
    PRINT 'Added ProviderAccessToken column'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderRefreshToken')
BEGIN
    ALTER TABLE Users ADD ProviderRefreshToken NVARCHAR(500) NULL;
    PRINT 'Added ProviderRefreshToken column'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProviderTokenExpires')
BEGIN
    ALTER TABLE Users ADD ProviderTokenExpires DATETIME2 NULL;
    PRINT 'Added ProviderTokenExpires column'
END

-- Add AdditionalRoles column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AdditionalRoles')
BEGIN
    ALTER TABLE Users ADD AdditionalRoles NVARCHAR(200) NULL;
    PRINT 'Added AdditionalRoles column'
END

-- Verify all columns exist
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' 
  AND COLUMN_NAME IN ('AuthProvider', 'ProviderUserId', 'ProviderAccessToken', 'ProviderRefreshToken', 'ProviderTokenExpires', 'AdditionalRoles')
ORDER BY COLUMN_NAME;

PRINT 'OAuth migration completed successfully!'
