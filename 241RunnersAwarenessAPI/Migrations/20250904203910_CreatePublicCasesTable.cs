using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAwarenessAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreatePublicCasesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmergencyContacts",
                table: "Runners");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalImageUrls",
                table: "Users",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentUrls",
                table: "Users",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Runners",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalImageUrls",
                table: "Runners",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgencyCaseNumber",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgencyCity",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgencyState",
                table: "Runners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChosenName",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Circumstances",
                table: "Runners",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityFound",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClothingDescription",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "County",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfLastContact",
                table: "Runners",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DentalStatus",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DistinctiveFeatures",
                table: "Runners",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DnaStatus",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentUrls",
                table: "Runners",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerprintStatus",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvestigatingAgency",
                table: "Runners",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeftEyeColor",
                table: "Runners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaidenName",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MannerOfDeath",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NcicNumber",
                table: "Runners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NcmecNumber",
                table: "Runners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalItems",
                table: "Runners",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Runners",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionStatus",
                table: "Runners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RightEyeColor",
                table: "Runners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateFound",
                table: "Runners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tribe",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleColor",
                table: "Runners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleMake",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleModel",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleVin",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleYear",
                table: "Runners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViCapNumber",
                table: "Runners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PublicCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NamusCaseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    AgeAtMissing = table.Column<int>(type: "int", nullable: true),
                    DateMissing = table.Column<DateTime>(type: "datetime2", nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    County = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Agency = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceLastChecked = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationSource = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicCases", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicCases");

            migrationBuilder.DropColumn(
                name: "AdditionalImageUrls",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DocumentUrls",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AdditionalImageUrls",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "AgencyCaseNumber",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "AgencyCity",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "AgencyState",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "ChosenName",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "Circumstances",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "CityFound",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "ClothingDescription",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "County",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "DateOfLastContact",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "DentalStatus",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "DistinctiveFeatures",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "DnaStatus",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "DocumentUrls",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "FingerprintStatus",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "InvestigatingAgency",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "LeftEyeColor",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "MaidenName",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "MannerOfDeath",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "NcicNumber",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "NcmecNumber",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "PersonalItems",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "ResolutionStatus",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "RightEyeColor",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "StateFound",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "Tribe",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "VehicleColor",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "VehicleMake",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "VehicleModel",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "VehicleVin",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "VehicleYear",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "ViCapNumber",
                table: "Runners");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Runners",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContacts",
                table: "Runners",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
