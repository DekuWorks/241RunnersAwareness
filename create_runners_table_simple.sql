-- Create Runners Table for 241 Runners Awareness API (Simplified Version)
-- This script creates the Runners table without foreign key constraints

USE [241runners-db];
GO

-- Create the Runners table
CREATE TABLE [dbo].[Runners] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [RunnerId] nvarchar(50) NOT NULL,
    [Age] int NOT NULL,
    [Gender] nvarchar(50) NULL,
    [Status] nvarchar(50) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [State] nvarchar(50) NOT NULL,
    [Address] nvarchar(500) NULL,
    [Description] nvarchar(500) NULL,
    [ContactInfo] nvarchar(200) NULL,
    [DateReported] datetime2 NOT NULL,
    [DateFound] datetime2 NULL,
    [LastSeen] datetime2 NULL,
    [DateOfBirth] datetime2 NULL,
    [Tags] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [IsUrgent] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [Height] nvarchar(50) NULL,
    [Weight] nvarchar(50) NULL,
    [HairColor] nvarchar(50) NULL,
    [EyeColor] nvarchar(50) NULL,
    [IdentifyingMarks] nvarchar(500) NULL,
    [MedicalConditions] nvarchar(1000) NULL,
    [Medications] nvarchar(500) NULL,
    [Allergies] nvarchar(500) NULL,
    [EmergencyContacts] nvarchar(500) NULL,
    [ReportedByUserId] int NULL,
    CONSTRAINT [PK_Runners] PRIMARY KEY ([Id])
);
GO

-- Create unique index on RunnerId
CREATE UNIQUE INDEX [IX_Runners_RunnerId] ON [dbo].[Runners] ([RunnerId]);
GO

-- Create index on ReportedByUserId for better query performance
CREATE INDEX [IX_Runners_ReportedByUserId] ON [dbo].[Runners] ([ReportedByUserId]);
GO

-- Create index on Status for filtering
CREATE INDEX [IX_Runners_Status] ON [dbo].[Runners] ([Status]);
GO

-- Create index on IsActive for filtering
CREATE INDEX [IX_Runners_IsActive] ON [dbo].[Runners] ([IsActive]);
GO

-- Create index on DateReported for sorting
CREATE INDEX [IX_Runners_DateReported] ON [dbo].[Runners] ([DateReported]);
GO

-- Insert sample data for testing
INSERT INTO [dbo].[Runners] (
    [FirstName], [LastName], [RunnerId], [Age], [Gender], [Status], 
    [City], [State], [Address], [Description], [ContactInfo], 
    [DateReported], [LastSeen], [Tags], [IsActive], [IsUrgent], 
    [CreatedAt], [Height], [Weight], [HairColor], [EyeColor], 
    [IdentifyingMarks], [MedicalConditions], [Medications], [Allergies], [EmergencyContacts]
) VALUES 
(
    'John', 'Smith', 'RUN-2024-001', 25, 'Male', 'missing',
    'Austin', 'TX', '123 Main St, Austin, TX 78701', 'Last seen running at Zilker Park',
    '555-123-4567', GETUTCDATE(), DATEADD(day, -1, GETUTCDATE()),
    'runner,missing,urgent', 1, 1, GETUTCDATE(),
    '5''10"', '160 lbs', 'Brown', 'Blue',
    'Tattoo on left forearm', 'None', 'None', 'None', 'Jane Smith (555-987-6543)'
),
(
    'Sarah', 'Johnson', 'RUN-2024-002', 32, 'Female', 'missing',
    'Houston', 'TX', '456 Oak Ave, Houston, TX 77001', 'Missing since morning run',
    '555-234-5678', GETUTCDATE(), DATEADD(day, -2, GETUTCDATE()),
    'runner,missing', 1, 0, GETUTCDATE(),
    '5''6"', '140 lbs', 'Blonde', 'Green',
    'Scar on right knee', 'Asthma', 'Inhaler', 'Pollen', 'Mike Johnson (555-876-5432)'
),
(
    'David', 'Wilson', 'RUN-2024-003', 28, 'Male', 'found',
    'Dallas', 'TX', '789 Pine St, Dallas, TX 75201', 'Found safe at local shelter',
    '555-345-6789', DATEADD(day, -5, GETUTCDATE()), DATEADD(day, -3, GETUTCDATE()),
    'runner,found', 1, 0, DATEADD(day, -5, GETUTCDATE()),
    '6''0"', '180 lbs', 'Black', 'Brown',
    'None', 'None', 'None', 'None', 'Lisa Wilson (555-765-4321)'
);
GO

-- Verify the table was created successfully
SELECT 
    'Runners table created successfully' as Status,
    COUNT(*) as TotalRunners
FROM [dbo].[Runners];
GO

-- Show table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Runners'
ORDER BY ORDINAL_POSITION;
GO 