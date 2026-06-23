using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add performance indexes (excluding Email which already exists)
            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastLoginAt",
                table: "Users",
                column: "LastLoginAt");

            migrationBuilder.CreateIndex(
                name: "IX_Runners_UserId",
                table: "Runners",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Runners_IsActive",
                table: "Runners",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_RunnerId",
                table: "Cases",
                column: "RunnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_Status",
                table: "Cases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CreatedAt",
                table: "Cases",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_LastSeenAt",
                table: "Cases",
                column: "LastSeenAt");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 13, 23, 27, 25, 276, DateTimeKind.Utc).AddTicks(2860), new DateTime(2025, 9, 13, 23, 27, 25, 276, DateTimeKind.Utc).AddTicks(2860), "$2a$11$cOsqewR/veybecDw85K6EeoPmxNTgACOVtfcx92q9m5bC//nqrRM2" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 13, 23, 27, 25, 394, DateTimeKind.Utc).AddTicks(8800), new DateTime(2025, 9, 13, 23, 27, 25, 394, DateTimeKind.Utc).AddTicks(8800), "$2a$11$xDsJMjQL.1Lh9ljRQ/NfXuWH0rCYusLy7e/IoPmPutoLeToC4CETi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop performance indexes
            migrationBuilder.DropIndex(
                name: "IX_Cases_LastSeenAt",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_CreatedAt",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_Status",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_RunnerId",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Runners_IsActive",
                table: "Runners");

            migrationBuilder.DropIndex(
                name: "IX_Runners_UserId",
                table: "Runners");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastLoginAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 13, 23, 10, 44, 607, DateTimeKind.Utc).AddTicks(4280), new DateTime(2025, 9, 13, 23, 10, 44, 607, DateTimeKind.Utc).AddTicks(4280), "$2a$11$wtbtBMklTzAlOZsrwEWkVOysGKczwiZMqXimdTF3YXiFjBoNL9Fia" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 13, 23, 10, 44, 722, DateTimeKind.Utc).AddTicks(7230), new DateTime(2025, 9, 13, 23, 10, 44, 722, DateTimeKind.Utc).AddTicks(7230), "$2a$11$7Nmy.sEBtVq8ZDcU80p0Tuj12znoGhALebZmjoeWEhKDk7hjcTJFG" });
        }
    }
}
