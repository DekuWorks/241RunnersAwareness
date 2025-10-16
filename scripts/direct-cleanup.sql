-- Direct SQL cleanup script for Azure SQL Database
-- This script removes all non-admin users and mock data

-- First, let's see what we have
SELECT 'Current Users:' as Info;
SELECT Id, Email, FirstName, LastName, Role, IsActive, CreatedAt 
FROM Users 
ORDER BY CreatedAt DESC;

-- Delete all users that are NOT in the admin list
DELETE FROM Users 
WHERE Email NOT IN (
    'dekuworks1@gmail.com',        -- Marcus Brown
    'danielcarey9770@yahoo.com',   -- Daniel Carey  
    'lthomas3350@gmail.com',       -- Lisa Thomas
    'tinaleggins@yahoo.com',       -- Tina Matthews
    'mmelasky@iplawconsulting.com', -- Mark Melasky
    'ralphfrank900@gmail.com'      -- Ralph Frank
);

-- Clean up related tables (if they exist)
-- Delete all cases
DELETE FROM Cases;

-- Delete all runners
DELETE FROM Runners;

-- Delete all notifications
DELETE FROM Notifications;

-- Delete all user sessions/tokens
DELETE FROM UserSessions;

-- Delete all audit logs (optional - you might want to keep these)
-- DELETE FROM AuditLogs;

-- Show final user count
SELECT 'Final Users:' as Info;
SELECT Id, Email, FirstName, LastName, Role, IsActive, CreatedAt 
FROM Users 
ORDER BY CreatedAt DESC;

-- Show counts
SELECT 'Final Counts:' as Info;
SELECT 
    (SELECT COUNT(*) FROM Users) as UserCount,
    (SELECT COUNT(*) FROM Cases) as CaseCount,
    (SELECT COUNT(*) FROM Runners) as RunnerCount,
    (SELECT COUNT(*) FROM Notifications) as NotificationCount;
