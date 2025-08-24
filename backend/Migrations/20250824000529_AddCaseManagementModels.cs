using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAwareness.BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseManagementModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CampaignType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GoalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Individuals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    LastKnownAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Height = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Weight = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HairColor = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    EyeColor = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DistinguishingFeatures = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PrimaryDisability = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DisabilityDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CommunicationMethod = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CommunicationNeeds = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsNonVerbal = table.Column<bool>(type: "INTEGER", nullable: true),
                    UsesAACDevice = table.Column<bool>(type: "INTEGER", nullable: true),
                    AACDeviceType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MobilityStatus = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UsesWheelchair = table.Column<bool>(type: "INTEGER", nullable: true),
                    UsesMobilityDevice = table.Column<bool>(type: "INTEGER", nullable: true),
                    MobilityDeviceType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HasVisualImpairment = table.Column<bool>(type: "INTEGER", nullable: true),
                    HasHearingImpairment = table.Column<bool>(type: "INTEGER", nullable: true),
                    HasSensoryProcessingDisorder = table.Column<bool>(type: "INTEGER", nullable: true),
                    SensoryTriggers = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SensoryComforts = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    BehavioralTriggers = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CalmingTechniques = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MayWanderOrElope = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsAttractedToWater = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsAttractedToRoads = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsAttractedToBrightLights = table.Column<bool>(type: "INTEGER", nullable: true),
                    WanderingPatterns = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PreferredLocations = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MedicalConditions = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Medications = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Allergies = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RequiresMedication = table.Column<bool>(type: "INTEGER", nullable: true),
                    MedicationSchedule = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    HasSeizureDisorder = table.Column<bool>(type: "INTEGER", nullable: true),
                    SeizureTriggers = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    HasDiabetes = table.Column<bool>(type: "INTEGER", nullable: true),
                    HasAsthma = table.Column<bool>(type: "INTEGER", nullable: true),
                    HasHeartCondition = table.Column<bool>(type: "INTEGER", nullable: true),
                    EmergencyResponseInstructions = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PreferredEmergencyContact = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ShouldCall911 = table.Column<bool>(type: "INTEGER", nullable: true),
                    SpecialInstructionsForFirstResponders = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    EnableRealTimeAlerts = table.Column<bool>(type: "INTEGER", nullable: true),
                    EnableSMSAlerts = table.Column<bool>(type: "INTEGER", nullable: true),
                    EnableEmailAlerts = table.Column<bool>(type: "INTEGER", nullable: true),
                    EnablePushNotifications = table.Column<bool>(type: "INTEGER", nullable: true),
                    AlertRadius = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AlertRadiusMiles = table.Column<int>(type: "INTEGER", nullable: true),
                    HasGPSDevice = table.Column<bool>(type: "INTEGER", nullable: true),
                    GPSDeviceType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    GPSDeviceID = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HasMedicalID = table.Column<bool>(type: "INTEGER", nullable: true),
                    MedicalIDNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CaregiverName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CaregiverPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CaregiverEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SupportOrganization = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SupportOrganizationPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    SpecialNeeds = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DNASample = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DNASampleType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DNASampleDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DNALabReference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DNASequence = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FingerprintData = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DentalRecords = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MedicalRecords = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SocialSecurityNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DriverLicenseNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PassportNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CaseStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CurrentStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LastSeenDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastSeenLocation = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Circumstances = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RiskLevel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsAtImmediateRisk = table.Column<bool>(type: "INTEGER", nullable: true),
                    RiskFactors = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NAMUSCaseNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LocalCaseNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InvestigatingAgency = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InvestigatorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InvestigatorPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    MediaReferences = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SocialMediaPosts = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SpecialNeedsDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    HasBeenAdopted = table.Column<bool>(type: "INTEGER", nullable: true),
                    PlacementStatus = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AdoptionDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Individuals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CustomerEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CustomerPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Shipping = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TrackingNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ShippingAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ShippingCity = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ShippingState = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ShippingZipCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ShippingCountry = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Partnerships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartnerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PartnerWebsite = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PartnershipType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LogoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Terms = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partnerships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IndividualId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlertType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AlertTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AlertMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    AlertStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AlertTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResolvedTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsUrgent = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlertRadiusMiles = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertLog_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IndividualId = table.Column<int>(type: "INTEGER", nullable: false),
                    DocumentUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DocumentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UploadedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseDocuments_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IndividualId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ImageType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UploadedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseImages_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Relationship = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IndividualId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyContacts_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EmailVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    PhoneVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EmailVerificationExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PhoneVerificationCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PhoneVerificationExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PasswordResetCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPasswordResetAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PasswordResetYear = table.Column<int>(type: "INTEGER", nullable: false),
                    PasswordResetToken = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RelationshipToRunner = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LicenseNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Organization = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Credentials = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Specialization = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    YearsOfExperience = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EmergencyContactRelationship = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    RefreshToken = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorSecret = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TwoFactorBackupCodes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TwoFactorSetupDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IndividualId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LongDescription = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CompareAtPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SKU = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Barcode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Subcategory = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Collection = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Size = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ImageUrls = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFeatured = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsOnSale = table.Column<bool>(type: "INTEGER", nullable: false),
                    StockQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    MinOrderQuantity = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxOrderQuantity = table.Column<int>(type: "INTEGER", nullable: true),
                    TrackInventory = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowBackorders = table.Column<bool>(type: "INTEGER", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WeightUnit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DimensionUnit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    AwarenessType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MissionStatement = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PartnerOrganization = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DonationPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DonationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MetaTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MetaDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CampaignId = table.Column<int>(type: "INTEGER", nullable: true),
                    PartnershipId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Partnerships_PartnershipId",
                        column: x => x.PartnershipId,
                        principalTable: "Partnerships",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CaseNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PublicSlug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IndividualId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    LastSeenLocation = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    LastSeenDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Circumstances = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    RiskLevel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsUrgent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResolvedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    LawEnforcementCaseNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    InvestigatingAgency = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    InvestigatorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    InvestigatorContact = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    SearchRadiusMiles = table.Column<int>(type: "INTEGER", nullable: true),
                    EnableAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnablePublicSharing = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableMediaOutreach = table.Column<bool>(type: "INTEGER", nullable: false),
                    SocialMediaHandles = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    MediaContacts = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cases_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cases_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DNAReports",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IndividualId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ReportDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DNASampleDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DNASampleType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DNASampleLocation = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DNASampleCollectionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DNALabReference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DNASequence = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DNASampleCollected = table.Column<bool>(type: "INTEGER", nullable: false),
                    DNASampleProcessed = table.Column<bool>(type: "INTEGER", nullable: false),
                    DNASampleMatched = table.Column<bool>(type: "INTEGER", nullable: false),
                    WeatherConditions = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ClothingDescription = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PhysicalDescription = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BehaviorDescription = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    WitnessName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    WitnessPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    WitnessEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ResolutionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ResolvedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DNAReports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_DNAReports_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DNAReports_Users_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AltText = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    ImageType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Size = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Style = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SKU = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    StockQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdateType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsUrgent = table.Column<bool>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RequiresNotification = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotificationsSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseUpdates_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseUpdates_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductVariantId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    VariantName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseUpdateMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CaseUpdateId = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MediaUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OriginalFilename = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UploadedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseUpdateMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseUpdateMedia_CaseUpdates_CaseUpdateId",
                        column: x => x.CaseUpdateId,
                        principalTable: "CaseUpdates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertLog_IndividualId",
                table: "AlertLog",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocuments_IndividualId",
                table: "CaseDocuments",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseImages_IndividualId",
                table: "CaseImages",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CaseNumber",
                table: "Cases",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CreatedAt",
                table: "Cases",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_IndividualId",
                table: "Cases",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_IsActive",
                table: "Cases",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_IsPublic",
                table: "Cases",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_LastUpdatedAt",
                table: "Cases",
                column: "LastUpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_OwnerUserId",
                table: "Cases",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_PublicSlug",
                table: "Cases",
                column: "PublicSlug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_Status",
                table: "Cases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CaseUpdateMedia_CaseUpdateId",
                table: "CaseUpdateMedia",
                column: "CaseUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseUpdateMedia_CreatedAt",
                table: "CaseUpdateMedia",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CaseUpdateMedia_MediaType",
                table: "CaseUpdateMedia",
                column: "MediaType");

            migrationBuilder.CreateIndex(
                name: "IX_CaseUpdates_CaseId",
                table: "CaseUpdates",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseUpdates_CreatedAt",
                table: "CaseUpdates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CaseUpdates_CreatedByUserId",
                table: "CaseUpdates",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseUpdates_IsPublic",
                table: "CaseUpdates",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_CaseUpdates_UpdateType",
                table: "CaseUpdates",
                column: "UpdateType");

            migrationBuilder.CreateIndex(
                name: "IX_DNAReports_CreatedAt",
                table: "DNAReports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DNAReports_IndividualId",
                table: "DNAReports",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_DNAReports_ReportDate",
                table: "DNAReports",
                column: "ReportDate");

            migrationBuilder.CreateIndex(
                name: "IX_DNAReports_ReporterUserId",
                table: "DNAReports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DNAReports_Status",
                table: "DNAReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_IndividualId",
                table: "EmergencyContacts",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_CaseStatus",
                table: "Individuals",
                column: "CaseStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_City",
                table: "Individuals",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_CreatedAt",
                table: "Individuals",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_LastSeenDate",
                table: "Individuals",
                column: "LastSeenDate");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_LocalCaseNumber",
                table: "Individuals",
                column: "LocalCaseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_Name",
                table: "Individuals",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_NAMUSCaseNumber",
                table: "Individuals",
                column: "NAMUSCaseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_State",
                table: "Individuals",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductVariantId",
                table: "OrderItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId",
                table: "ProductReviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CampaignId",
                table: "Products",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PartnershipId",
                table: "Products",
                column: "PartnershipId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IndividualId",
                table: "Users",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertLog");

            migrationBuilder.DropTable(
                name: "CaseDocuments");

            migrationBuilder.DropTable(
                name: "CaseImages");

            migrationBuilder.DropTable(
                name: "CaseUpdateMedia");

            migrationBuilder.DropTable(
                name: "DNAReports");

            migrationBuilder.DropTable(
                name: "EmergencyContacts");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "ProductReviews");

            migrationBuilder.DropTable(
                name: "CaseUpdates");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Partnerships");

            migrationBuilder.DropTable(
                name: "Individuals");
        }
    }
}
