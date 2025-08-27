-- SQL Server Database Schema for 241 Runners Awareness
-- This script creates all the necessary tables for the application

-- Create database schema for 241 Runners Awareness

-- Create Users table
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    FullName NVARCHAR(200) NOT NULL,
    PhoneNumber NVARCHAR(20),
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'User',
    IsActive BIT NOT NULL DEFAULT 1,
    EmailVerified BIT NOT NULL DEFAULT 0,
    PhoneVerified BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    TwoFactorSecret NVARCHAR(500),
    BackupCodes NVARCHAR(MAX),
    LastLoginDate DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Create Individuals table
CREATE TABLE Individuals (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    MiddleName NVARCHAR(100),
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10),
    LastKnownAddress NVARCHAR(200),
    Address NVARCHAR(200),
    City NVARCHAR(100),
    State NVARCHAR(50),
    ZipCode NVARCHAR(10),
    Latitude FLOAT,
    Longitude FLOAT,
    PhoneNumber NVARCHAR(20),
    Email NVARCHAR(100),
    Height NVARCHAR(50),
    Weight NVARCHAR(50),
    HairColor NVARCHAR(50),
    EyeColor NVARCHAR(50),
    DistinguishingFeatures NVARCHAR(200),
    PrimaryDisability NVARCHAR(100),
    DisabilityDescription NVARCHAR(500),
    CommunicationMethod NVARCHAR(100),
    CommunicationNeeds NVARCHAR(200),
    IsNonVerbal BIT,
    UsesAACDevice BIT,
    AACDeviceType NVARCHAR(100),
    MobilityStatus NVARCHAR(100),
    UsesWheelchair BIT,
    UsesMobilityDevice BIT,
    MobilityDeviceType NVARCHAR(100),
    HasVisualImpairment BIT,
    HasHearingImpairment BIT,
    HasSensoryProcessingDisorder BIT,
    SensoryTriggers NVARCHAR(200),
    SensoryComforts NVARCHAR(200),
    BehavioralTriggers NVARCHAR(200),
    CalmingTechniques NVARCHAR(200),
    MayWanderOrElope BIT,
    IsAttractedToWater BIT,
    IsAttractedToRoads BIT,
    IsAttractedToBrightLights BIT,
    WanderingPatterns NVARCHAR(200),
    PreferredLocations NVARCHAR(200),
    MedicalConditions NVARCHAR(200),
    Medications NVARCHAR(200),
    Allergies NVARCHAR(200),
    RequiresMedication BIT,
    MedicationSchedule NVARCHAR(200),
    HasSeizureDisorder BIT,
    SeizureTriggers NVARCHAR(200),
    HasDiabetes BIT,
    HasAsthma BIT,
    HasHeartCondition BIT,
    EmergencyResponseInstructions NVARCHAR(200),
    PreferredEmergencyContact NVARCHAR(200),
    ShouldCall911 BIT,
    SpecialInstructionsForFirstResponders NVARCHAR(200),
    EnableRealTimeAlerts BIT,
    EnableSMSAlerts BIT,
    EnableEmailAlerts BIT,
    SpecialNeeds NVARCHAR(500),
    LastSeenDate DATETIME2,
    LastSeenLocation NVARCHAR(200),
    Circumstances NVARCHAR(500),
    CaseStatus NVARCHAR(50) DEFAULT 'Active',
    NAMUSCaseNumber NVARCHAR(100),
    LocalCaseNumber NVARCHAR(100),
    InvestigatingAgency NVARCHAR(100),
    InvestigatorName NVARCHAR(100),
    InvestigatorPhone NVARCHAR(20),
    MediaReferences NVARCHAR(500),
    SocialMediaPosts NVARCHAR(500),
    DNASample NVARCHAR(500),
    DNASampleType NVARCHAR(100),
    DNALabReference NVARCHAR(100),
    DNASequence NVARCHAR(500),
    FingerprintData NVARCHAR(200),
    DentalRecords NVARCHAR(200),
    MedicalRecords NVARCHAR(200),
    SocialSecurityNumber NVARCHAR(100),
    DriverLicenseNumber NVARCHAR(100),
    PassportNumber NVARCHAR(100),
    CreatedBy NVARCHAR(100),
    UpdatedBy NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Create EmergencyContacts table
CREATE TABLE EmergencyContacts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IndividualId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100),
    Relationship NVARCHAR(100),
    IsPrimary BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (IndividualId) REFERENCES Individuals(Id) ON DELETE CASCADE
);

-- Create Cases table
CREATE TABLE Cases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CaseNumber NVARCHAR(100) NOT NULL UNIQUE,
    PublicSlug NVARCHAR(100) NOT NULL UNIQUE,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open',
    Priority NVARCHAR(50) NOT NULL DEFAULT 'Medium',
    RiskLevel NVARCHAR(50) NOT NULL DEFAULT 'Medium',
    Category NVARCHAR(100),
    Tags NVARCHAR(500),
    Location NVARCHAR(200),
    Latitude FLOAT,
    Longitude FLOAT,
    LastSeenLocation NVARCHAR(200),
    Circumstances NVARCHAR(500),
    ResolutionNotes NVARCHAR(MAX),
    LawEnforcementCaseNumber NVARCHAR(100),
    InvestigatingAgency NVARCHAR(100),
    InvestigatorName NVARCHAR(100),
    InvestigatorContact NVARCHAR(100),
    SocialMediaHandles NVARCHAR(500),
    MediaContacts NVARCHAR(500),
    IsPublic BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    IndividualId INT,
    OwnerUserId INT,
    CreatedBy NVARCHAR(100),
    UpdatedBy NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (IndividualId) REFERENCES Individuals(Id),
    FOREIGN KEY (OwnerUserId) REFERENCES Users(Id)
);

-- Create CaseUpdates table
CREATE TABLE CaseUpdates (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CaseId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    UpdateType NVARCHAR(50) NOT NULL,
    IsPublic BIT NOT NULL DEFAULT 1,
    CreatedBy NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CaseId) REFERENCES Cases(Id) ON DELETE CASCADE
);

-- Create DNAReports table
CREATE TABLE DNAReports (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReportTitle NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    ReportType NVARCHAR(100),
    Location NVARCHAR(200),
    DateFound DATETIME2,
    SampleType NVARCHAR(100),
    SampleCondition NVARCHAR(100),
    LabReference NVARCHAR(100),
    DNASequence NVARCHAR(500),
    AnalysisResults NVARCHAR(MAX),
    MatchConfidence DECIMAL(5,2),
    MatchedIndividualId INT,
    Status NVARCHAR(50) DEFAULT 'Pending',
    IsPublic BIT NOT NULL DEFAULT 0,
    ReporterId INT,
    IndividualId INT,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (MatchedIndividualId) REFERENCES Individuals(Id),
    FOREIGN KEY (ReporterId) REFERENCES Users(Id),
    FOREIGN KEY (IndividualId) REFERENCES Individuals(Id)
);

-- Create Products table
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL,
    Category NVARCHAR(100),
    Brand NVARCHAR(100),
    SKU NVARCHAR(100) UNIQUE,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Create ProductVariants table
CREATE TABLE ProductVariants (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    VariantName NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2),
    StockQuantity INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

-- Create ProductImages table
CREATE TABLE ProductImages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    AltText NVARCHAR(200),
    IsPrimary BIT NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

-- Create ProductReviews table
CREATE TABLE ProductReviews (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Title NVARCHAR(200),
    Comment NVARCHAR(MAX),
    IsVerified BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Create Orders table
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber NVARCHAR(100) NOT NULL UNIQUE,
    CustomerEmail NVARCHAR(100) NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    CustomerPhone NVARCHAR(20),
    ShippingAddress NVARCHAR(500),
    BillingAddress NVARCHAR(500),
    Subtotal DECIMAL(18,2) NOT NULL,
    Tax DECIMAL(18,2) NOT NULL DEFAULT 0,
    Shipping DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    PaymentMethod NVARCHAR(100),
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Create OrderItems table
CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductVariantId INT,
    ProductName NVARCHAR(200) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id),
    FOREIGN KEY (ProductVariantId) REFERENCES ProductVariants(Id)
);

-- Create Partnerships table
CREATE TABLE Partnerships (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PartnerName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Website NVARCHAR(500),
    ContactEmail NVARCHAR(100),
    ContactPhone NVARCHAR(20),
    PartnershipType NVARCHAR(100),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Create Campaigns table
CREATE TABLE Campaigns (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    CampaignType NVARCHAR(100),
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2,
    GoalAmount DECIMAL(18,2),
    CurrentAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    ImageUrl NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Create CaseImages table
CREATE TABLE CaseImages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CaseId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    ImageType NVARCHAR(50),
    Description NVARCHAR(500),
    IsPrimary BIT NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CaseId) REFERENCES Cases(Id) ON DELETE CASCADE
);

-- Create CaseDocuments table
CREATE TABLE CaseDocuments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CaseId INT NOT NULL,
    DocumentUrl NVARCHAR(500) NOT NULL,
    DocumentType NVARCHAR(50),
    Title NVARCHAR(200),
    Description NVARCHAR(500),
    FileSize BIGINT,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CaseId) REFERENCES Cases(Id) ON DELETE CASCADE
);

-- Create CaseUpdateMedia table
CREATE TABLE CaseUpdateMedia (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CaseUpdateId INT NOT NULL,
    MediaUrl NVARCHAR(500) NOT NULL,
    MediaType NVARCHAR(50),
    Description NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CaseUpdateId) REFERENCES CaseUpdates(Id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX IX_Individuals_CaseStatus ON Individuals(CaseStatus);
CREATE INDEX IX_Individuals_State ON Individuals(State);
CREATE INDEX IX_Individuals_City ON Individuals(City);
CREATE INDEX IX_Individuals_LastSeenDate ON Individuals(LastSeenDate);
CREATE INDEX IX_Individuals_CreatedAt ON Individuals(CreatedAt);
CREATE INDEX IX_Individuals_Name ON Individuals(FirstName, LastName);
CREATE INDEX IX_Individuals_NAMUSCaseNumber ON Individuals(NAMUSCaseNumber);
CREATE INDEX IX_Individuals_LocalCaseNumber ON Individuals(LocalCaseNumber);

CREATE INDEX IX_Cases_Status ON Cases(Status);
CREATE INDEX IX_Cases_Priority ON Cases(Priority);
CREATE INDEX IX_Cases_CreatedAt ON Cases(CreatedAt);
CREATE INDEX IX_Cases_IndividualId ON Cases(IndividualId);

CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Username ON Users(Username);

-- Insert default admin user (commented out for now)
-- INSERT INTO Users (Username, Email, FullName, PasswordHash, Role, IsActive, EmailVerified)
-- VALUES ('admin', 'admin@241runnersawareness.org', 'System Administrator', 
--         '$2a$11$YourHashedPasswordHere', 'Admin', 1, 1);
