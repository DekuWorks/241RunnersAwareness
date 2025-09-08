using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Organization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Credentials = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    YearsOfExperience = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalImageUrls = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DocumentUrls = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactRelationship = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsPhoneVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmailVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhoneVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailVerificationToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordResetTokenExpires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AdditionalImageUrls", "Address", "City", "CreatedAt", "Credentials", "DocumentUrls", "Email", "EmailVerificationToken", "EmailVerifiedAt", "EmergencyContactName", "EmergencyContactPhone", "EmergencyContactRelationship", "FirstName", "IsActive", "IsEmailVerified", "LastLoginAt", "LastName", "LockedUntil", "Notes", "Organization", "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpires", "PhoneNumber", "PhoneVerifiedAt", "ProfileImageUrl", "Role", "Specialization", "State", "Title", "UpdatedAt", "YearsOfExperience", "ZipCode" },
                values: new object[,]
                {
                    { 1, null, null, null, new DateTime(2025, 9, 8, 14, 34, 33, 850, DateTimeKind.Utc).AddTicks(2490), null, null, "admin@241runnersawareness.org", null, new DateTime(2025, 9, 8, 14, 34, 33, 850, DateTimeKind.Utc).AddTicks(2490), null, null, null, "System", true, true, null, "Administrator", null, null, null, "$2a$11$EJWcR45ghE5owH/9CtmKReTMeEQ.LzHCDrD2/EOh5agerXj4l8DOS", null, null, null, null, null, "admin", null, null, null, null, null, null },
                    { 2, null, null, null, new DateTime(2025, 9, 8, 14, 34, 33, 965, DateTimeKind.Utc).AddTicks(3640), null, null, "support@241runnersawareness.org", null, new DateTime(2025, 9, 8, 14, 34, 33, 965, DateTimeKind.Utc).AddTicks(3640), null, null, null, "Support", true, true, null, "Team", null, null, null, "$2a$11$FGBTQ9ZhVwc8B.w.ybKzAeppp95/b9sxP5zy9VUCnVdGAoMgDXCeO", null, null, null, null, null, "admin", null, null, null, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailVerificationToken",
                table: "Users",
                column: "EmailVerificationToken",
                unique: true,
                filter: "[EmailVerificationToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PasswordResetToken",
                table: "Users",
                column: "PasswordResetToken",
                unique: true,
                filter: "[PasswordResetToken] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
