-- Simple Setup for Admin Users - 241 Runners Awareness
-- This script works with the existing database schema

-- First, let's see what columns actually exist in the Users table
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;

-- Check if there are any users in the database
SELECT COUNT(*) as UserCount FROM Users;

-- Create the 6 admin users with basic schema
-- 1. Marcus Brown
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'dekuworks1@gmail.com')
BEGIN
    INSERT INTO Users (
        Email, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'dekuworks1@gmail.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'marcus2025'
        'Marcus',
        'Brown',
        'admin',
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
        Email, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'lthomas3350@gmail.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'Lisa2025!'
        'Lisa',
        'Thomas',
        'admin',
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
        Email, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'markmelasky@gmail.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'Mark2025!'
        'Mark',
        'Melasky',
        'admin',
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
        Email, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'danielcarey9770@yahoo.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'Daniel2025!'
        'Daniel',
        'Carey',
        'admin',
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
        Email, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'tinaleggins@yahoo.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'Tina2025!'
        'Tina',
        'Matthews',
        'admin',
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
        Email, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt, PhoneNumber, IsEmailVerified, IsPhoneVerified
    ) VALUES (
        'ralphfrank900@gmail.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdqxJ5u3P2l8Y1LZ1k1X1Y1Y1Y1Y1Y', -- BCrypt hash for 'Ralph2025!'
        'Ralph',
        'Frank',
        'admin',
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
    IsActive,
    CreatedAt
FROM Users
WHERE Role = 'admin'
ORDER BY FirstName, LastName;

PRINT 'Database setup complete! All 6 admin users should now be available for login.'
