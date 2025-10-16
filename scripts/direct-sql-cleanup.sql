-- Complete Database Cleanup SQL Script
-- This script removes ALL non-admin users, ALL runners, and ALL cases
-- Only keeps the 6 designated admin users

-- First, let's see what we have before cleanup
SELECT 'BEFORE CLEANUP - Users:' as Info;
SELECT Id, Email, FirstName, LastName, Role, IsActive FROM Users ORDER BY Role, Email;

SELECT 'BEFORE CLEANUP - Runners:' as Info;
SELECT Id, Name, FirstName, LastName, UserId FROM Runners ORDER BY Id;

SELECT 'BEFORE CLEANUP - Cases:' as Info;
SELECT Id, Title, Status, CreatedAt FROM Cases ORDER BY Id;

-- Delete all runners first (due to foreign key constraints)
DELETE FROM Runners;

-- Delete all cases (due to foreign key constraints)
DELETE FROM Cases;

-- Delete all notifications (due to foreign key constraints)
DELETE FROM Notifications;

-- Delete all non-admin users
-- Keep only these 6 admin emails:
-- dekuworks1@gmail.com (Marcus Brown)
-- danielcarey9770@yahoo.com (Daniel Carey)
-- lthomas3350@gmail.com (Lisa Thomas)
-- tinaleggins@yahoo.com (Tina Matthews)
-- mmelasky@iplawconsulting.com (Mark Melasky)
-- ralphfrank900@gmail.com (Ralph Frank)

DELETE FROM Users 
WHERE Email NOT IN (
    'dekuworks1@gmail.com',
    'danielcarey9770@yahoo.com',
    'lthomas3350@gmail.com',
    'tinaleggins@yahoo.com',
    'mmelasky@iplawconsulting.com',
    'ralphfrank900@gmail.com'
);

-- Reset identity columns to start from 1
DBCC CHECKIDENT ('Users', RESEED, 6);
DBCC CHECKIDENT ('Runners', RESEED, 0);
DBCC CHECKIDENT ('Cases', RESEED, 0);
DBCC CHECKIDENT ('Notifications', RESEED, 0);

-- Show final results
SELECT 'AFTER CLEANUP - Users:' as Info;
SELECT Id, Email, FirstName, LastName, Role, IsActive FROM Users ORDER BY Role, Email;

SELECT 'AFTER CLEANUP - Runners:' as Info;
SELECT COUNT(*) as RunnerCount FROM Runners;

SELECT 'AFTER CLEANUP - Cases:' as Info;
SELECT COUNT(*) as CaseCount FROM Cases;

SELECT 'AFTER CLEANUP - Notifications:' as Info;
SELECT COUNT(*) as NotificationCount FROM Notifications;

-- Final verification
SELECT 
    'CLEANUP SUMMARY' as Info,
    (SELECT COUNT(*) FROM Users) as TotalUsers,
    (SELECT COUNT(*) FROM Runners) as TotalRunners,
    (SELECT COUNT(*) FROM Cases) as TotalCases,
    (SELECT COUNT(*) FROM Notifications) as TotalNotifications;
