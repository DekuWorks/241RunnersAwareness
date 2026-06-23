using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDataDeletionRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountDeletionRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessingNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountDeletionRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountDeletionRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataDeletionRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DataTypes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessingNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataDeletionRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataDeletionRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 13, 22, 52, 43, 215, DateTimeKind.Utc).AddTicks(7570), new DateTime(2025, 10, 13, 22, 52, 43, 215, DateTimeKind.Utc).AddTicks(7570), "$2a$11$dWOMft3nZQNG9RCqswrMnuzMQgo.NdTk.m4RhMsbz0nq9dks.v7/y" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 13, 22, 52, 43, 402, DateTimeKind.Utc).AddTicks(9680), new DateTime(2025, 10, 13, 22, 52, 43, 402, DateTimeKind.Utc).AddTicks(9680), "$2a$11$mGXGuoLdWCPLTvhlNI1p..hZC4uL1qzpisnhq5JwNbxHSRzMdbUE2" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountDeletionRequests_RequestedAt",
                table: "AccountDeletionRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDeletionRequests_Status",
                table: "AccountDeletionRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDeletionRequests_UserId",
                table: "AccountDeletionRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataDeletionRequests_RequestedAt",
                table: "DataDeletionRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DataDeletionRequests_Status",
                table: "DataDeletionRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DataDeletionRequests_UserId",
                table: "DataDeletionRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountDeletionRequests");

            migrationBuilder.DropTable(
                name: "DataDeletionRequests");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 7, 23, 45, 7, 646, DateTimeKind.Utc).AddTicks(7850), new DateTime(2025, 10, 7, 23, 45, 7, 646, DateTimeKind.Utc).AddTicks(7850), "$2a$11$QJmvl5/mPOYw2bH8iwbNQ.KPpiGN/3/V9AYL6.L15mzr8VrObyx96" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 7, 23, 45, 7, 759, DateTimeKind.Utc).AddTicks(1160), new DateTime(2025, 10, 7, 23, 45, 7, 759, DateTimeKind.Utc).AddTicks(1160), "$2a$11$M/lqaoDzi9RczRPB/QtmWeRvVbZx8KeLH7JmdxF3IQFCuDpvwipJe" });
        }
    }
}
