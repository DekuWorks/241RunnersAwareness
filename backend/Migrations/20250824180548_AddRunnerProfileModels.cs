using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAwareness.BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRunnerProfileModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "Individuals",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserUserId",
                table: "Individuals",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RunnerId",
                table: "Individuals",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Individuals",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IndividualId1",
                table: "Cases",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IndividualId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Caption = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ImageType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UploadedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: true),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IndividualId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActivityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RelatedCaseId = table.Column<int>(type: "INTEGER", nullable: true),
                    RelatedPhotoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_Cases_RelatedCaseId",
                        column: x => x.RelatedCaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Activities_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Photos_RelatedPhotoId",
                        column: x => x.RelatedPhotoId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_OwnerUserUserId",
                table: "Individuals",
                column: "OwnerUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_IndividualId1",
                table: "Cases",
                column: "IndividualId1");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ActivityType",
                table: "Activities",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_CreatedAt",
                table: "Activities",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_IndividualId",
                table: "Activities",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_RelatedCaseId",
                table: "Activities",
                column: "RelatedCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_RelatedPhotoId",
                table: "Activities",
                column: "RelatedPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_IndividualId",
                table: "Photos",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_IsPrimary",
                table: "Photos",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_UploadedAt",
                table: "Photos",
                column: "UploadedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Individuals_IndividualId1",
                table: "Cases",
                column: "IndividualId1",
                principalTable: "Individuals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Individuals_Users_OwnerUserUserId",
                table: "Individuals",
                column: "OwnerUserUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Individuals_IndividualId1",
                table: "Cases");

            migrationBuilder.DropForeignKey(
                name: "FK_Individuals_Users_OwnerUserUserId",
                table: "Individuals");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Individuals_OwnerUserUserId",
                table: "Individuals");

            migrationBuilder.DropIndex(
                name: "IX_Cases_IndividualId1",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "OwnerUserUserId",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "RunnerId",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "IndividualId1",
                table: "Cases");
        }
    }
}
