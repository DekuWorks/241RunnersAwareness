using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRunnerStatusField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Runners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Missing");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 5, 3, 10, 30, 324, DateTimeKind.Utc).AddTicks(4840), new DateTime(2025, 10, 5, 3, 10, 30, 324, DateTimeKind.Utc).AddTicks(4840), "$2a$11$4Ec59n61qDG084BXAeo3Q..o1eWDpWw5hY1bOa7GuKvoAwLzO8DKC" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 5, 3, 10, 30, 443, DateTimeKind.Utc).AddTicks(3050), new DateTime(2025, 10, 5, 3, 10, 30, 443, DateTimeKind.Utc).AddTicks(3050), "$2a$11$VKDK/Gxx8xFdLwTrs6aAQ.gnQc9BeINEKiOFmvkPDkqcyiYUm7yqC" });

            migrationBuilder.CreateIndex(
                name: "IX_Runners_Status",
                table: "Runners",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Runners_Status",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Runners");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 27, 15, 46, 53, 850, DateTimeKind.Utc).AddTicks(5890), new DateTime(2025, 9, 27, 15, 46, 53, 850, DateTimeKind.Utc).AddTicks(5900), "$2a$11$ZkKx1sIsLcaVBZc8SlHwTuTG3V4nCGGGbBYc6tPbTrP31l1Vp7X9a" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 27, 15, 46, 53, 961, DateTimeKind.Utc).AddTicks(3090), new DateTime(2025, 9, 27, 15, 46, 53, 961, DateTimeKind.Utc).AddTicks(3090), "$2a$11$QDwuADBosSkIAYfJZ8.G2.3ceZK48U6erbR1viw/XsNHQxTeBRuka" });
        }
    }
}
