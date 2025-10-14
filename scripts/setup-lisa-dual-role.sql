-- Setup Lisa Thomas with dual admin/user roles
-- This allows her to function as both an admin and a regular user
-- so she can create runners and manage cases for her family

-- Update Lisa Thomas's account to have additional roles
-- Primary role: admin (already set)
-- Additional roles: user, parent (for family management)

UPDATE Users 
SET AdditionalRoles = '["user", "parent"]'
WHERE Email = 'lthomas3350@gmail.com';

-- Verify the update
SELECT 
    Id,
    Email,
    FirstName,
    LastName,
    Role,
    AdditionalRoles,
    IsActive,
    CreatedAt
FROM Users 
WHERE Email = 'lthomas3350@gmail.com';

-- Optional: Set up other admins with dual roles if needed
-- Uncomment and modify as needed:

/*
-- Update Marcus Brown (if he wants dual roles)
UPDATE Users 
SET AdditionalRoles = '["user", "parent"]'
WHERE Email = 'dekuworks1@gmail.com';

-- Update Daniel Carey (if he wants dual roles)
UPDATE Users 
SET AdditionalRoles = '["user", "parent"]'
WHERE Email = 'danielcarey9770@yahoo.com';

-- Update Tina Matthews (if she wants dual roles)
UPDATE Users 
SET AdditionalRoles = '["user", "parent"]'
WHERE Email = 'tinaleggins@yahoo.com';

-- Update Ralph Frank (if he wants dual roles)
UPDATE Users 
SET AdditionalRoles = '["user", "parent"]'
WHERE Email = 'ralphfrank900@gmail.com';
*/

-- Show all admin users and their roles
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
