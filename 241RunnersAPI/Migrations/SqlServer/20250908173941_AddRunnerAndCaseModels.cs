using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRunnerAndCaseModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Runners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PhysicalDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MedicalConditions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Medications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmergencyInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreferredRunningLocations = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TypicalRunningTimes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExperienceLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpecialNeeds = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastKnownLocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastLocationUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreferredContactMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalImageUrls = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsProfileComplete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Runners_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RunnerId = table.Column<int>(type: "int", nullable: false),
                    ReportedByUserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LastSeenDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LastSeenTime = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastSeenCircumstances = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ClothingDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhysicalCondition = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MentalState = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AdditionalInformation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPersonName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPersonPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactPersonEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LastSeenLatitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastSeenLongitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CaseImageUrls = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DocumentUrls = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactRelationship = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ShareCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TipCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cases_Runners_RunnerId",
                        column: x => x.RunnerId,
                        principalTable: "Runners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cases_Users_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 8, 17, 39, 40, 920, DateTimeKind.Utc).AddTicks(4160), new DateTime(2025, 9, 8, 17, 39, 40, 920, DateTimeKind.Utc).AddTicks(4160), "$2a$11$xVLK9sMknmIw8K8PZRpshuDQ43t29bWJZ1A3NLPdnIdQe9lhoRF5a" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 8, 17, 39, 41, 37, DateTimeKind.Utc).AddTicks(2600), new DateTime(2025, 9, 8, 17, 39, 41, 37, DateTimeKind.Utc).AddTicks(2600), "$2a$11$NX55S9dDm0f8uW.csD2M9.fezyTyZLRcw3.owd8dxPwE3MWoyKfZO" });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_ReportedByUserId",
                table: "Cases",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_RunnerId",
                table: "Cases",
                column: "RunnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Runners_UserId",
                table: "Runners",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "Runners");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 8, 14, 34, 33, 850, DateTimeKind.Utc).AddTicks(2490), new DateTime(2025, 9, 8, 14, 34, 33, 850, DateTimeKind.Utc).AddTicks(2490), "$2a$11$EJWcR45ghE5owH/9CtmKReTMeEQ.LzHCDrD2/EOh5agerXj4l8DOS" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 8, 14, 34, 33, 965, DateTimeKind.Utc).AddTicks(3640), new DateTime(2025, 9, 8, 14, 34, 33, 965, DateTimeKind.Utc).AddTicks(3640), "$2a$11$FGBTQ9ZhVwc8B.w.ybKzAeppp95/b9sxP5zy9VUCnVdGAoMgDXCeO" });
        }
    }
}
