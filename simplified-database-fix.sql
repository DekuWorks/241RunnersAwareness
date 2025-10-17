-- SIMPLIFIED DATABASE FIX - Execute one table at a time
-- This script creates missing tables one by one to avoid errors

-- Step 1: Create Cases table
CREATE TABLE Cases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CaseNumber NVARCHAR(50) NOT NULL UNIQUE,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open',
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium',
    Category NVARCHAR(100),
    Location NVARCHAR(255),
    Latitude DECIMAL(10,8),
    Longitude DECIMAL(11,8),
    CreatedBy INT NOT NULL,
    AssignedTo INT,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    ClosedAt DATETIME2,
    IsPublic BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    Tags NVARCHAR(500),
    Notes NVARCHAR(MAX),
    Evidence NVARCHAR(MAX),
    ContactInfo NVARCHAR(255),
    ContactPhone NVARCHAR(20),
    ContactEmail NVARCHAR(255),
    Resolution NVARCHAR(MAX)
);

-- Add foreign key constraints after table creation
ALTER TABLE Cases ADD CONSTRAINT FK_Cases_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
ALTER TABLE Cases ADD CONSTRAINT FK_Cases_AssignedTo FOREIGN KEY (AssignedTo) REFERENCES Users(Id);
