-- Seed admin users for 241 Runners Awareness
-- Run this script against the Azure SQL database

-- Check if admin users already exist
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@241runnersawareness.org')
BEGIN
    INSERT INTO Users (
        Email, PasswordHash, FirstName, LastName, Role, IsActive, IsEmailVerified, IsPhoneVerified,
        CreatedAt, EmailVerifiedAt, PhoneVerifiedAt, Organization, Title, PhoneNumber,
        Address, City, State, ZipCode, EmergencyContactName, EmergencyContactPhone, EmergencyContactRelationship
    ) VALUES (
        'admin@241runnersawareness.org',
        '$2a$11$EJWcR45ghE5owH/9CtmKReTMeEQ.LzHCDrD2/EOh5agerXj4l8DOS', -- Admin@241Runners2024!
        'System',
        'Administrator',
        'admin',
        1,
        1,
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        GETUTCDATE(),
        '241 Runners Awareness',
        'System Administrator',
        '+1-555-0123',
        '123 Safety Street',
        'Awareness City',
        'Safety State',
        '12345',
        'Emergency Services',
        '+1-555-911',
        'Emergency Contact'
    );
    PRINT 'Admin user created: admin@241runnersawareness.org';
END
ELSE
BEGIN
    PRINT 'Admin user already exists: admin@241runnersawareness.org';
END

-- Check if support user already exists
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'support@241runnersawareness.org')
BEGIN
    INSERT INTO Users (
        Email, PasswordHash, FirstName, LastName, Role, IsActive, IsEmailVerified, IsPhoneVerified,
        CreatedAt, EmailVerifiedAt, PhoneVerifiedAt, Organization, Title, PhoneNumber,
        Address, City, State, ZipCode, EmergencyContactName, EmergencyContactPhone, EmergencyContactRelationship
    ) VALUES (
        'support@241runnersawareness.org',
        '$2a$11$FGBTQ9ZhVwc8B.w.ybKzAeppp95/b9sxP5zy9VUCnVdGAoMgDXCeO', -- Support@241Runners2024!
        'Support',
        'Team',
        'admin',
        1,
        1,
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        GETUTCDATE(),
        '241 Runners Awareness',
        'Support Administrator',
        '+1-555-0124',
        '123 Support Avenue',
        'Help City',
        'Support State',
        '12346',
        'Emergency Services',
        '+1-555-911',
        'Emergency Contact'
    );
    PRINT 'Support user created: support@241runnersawareness.org';
END
ELSE
BEGIN
    PRINT 'Support user already exists: support@241runnersawareness.org';
END

-- Check if test user already exists
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'test@241runnersawareness.org')
BEGIN
    INSERT INTO Users (
        Email, PasswordHash, FirstName, LastName, Role, IsActive, IsEmailVerified, IsPhoneVerified,
        CreatedAt, EmailVerifiedAt, PhoneVerifiedAt, Organization, Title, PhoneNumber,
        Address, City, State, ZipCode, EmergencyContactName, EmergencyContactPhone, EmergencyContactRelationship
    ) VALUES (
        'test@241runnersawareness.org',
        '$2a$11$TestHashForTestUser123456789012345678901234567890123456789012345678901234567890', -- Test@241Runners2024!
        'Test',
        'User',
        'user',
        1,
        1,
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        GETUTCDATE(),
        'Test Organization',
        'Test User',
        '+1-555-0125',
        '123 Test Street',
        'Test City',
        'Test State',
        '12347',
        'Test Emergency Contact',
        '+1-555-0126',
        'Family'
    );
    PRINT 'Test user created: test@241runnersawareness.org';
END
ELSE
BEGIN
    PRINT 'Test user already exists: test@241runnersawareness.org';
END

-- Show all users
SELECT Id, Email, FirstName, LastName, Role, IsActive, IsEmailVerified, CreatedAt FROM Users ORDER BY Id;
