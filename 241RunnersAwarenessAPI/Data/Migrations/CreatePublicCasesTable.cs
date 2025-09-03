using Microsoft.EntityFrameworkCore.Migrations;

namespace _241RunnersAwarenessAPI.Data.Migrations
{
    public partial class CreatePublicCasesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "missing"),
                    StatusNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceLastChecked = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationSource = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicCases", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicCases_NamusCaseNumber",
                table: "PublicCases",
                column: "NamusCaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicCases_Status",
                table: "PublicCases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PublicCases_State",
                table: "PublicCases",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_PublicCases_City",
                table: "PublicCases",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_PublicCases_County",
                table: "PublicCases",
                column: "County");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicCases");
        }
    }
} 