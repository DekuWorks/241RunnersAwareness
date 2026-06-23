using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEnhancedRunnerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EyeColor",
                table: "Runners",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Height",
                table: "Runners",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPhotoUpdate",
                table: "Runners",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextPhotoReminder",
                table: "Runners",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhotoUpdateReminderCount",
                table: "Runners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PhotoUpdateReminderSent",
                table: "Runners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Weight",
                table: "Runners",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 7, 23, 44, 19, 622, DateTimeKind.Utc).AddTicks(7870), new DateTime(2025, 10, 7, 23, 44, 19, 622, DateTimeKind.Utc).AddTicks(7870), "$2a$11$AFJdIGqQsVsEpHsnOlDO9ONvGZun6dM4jhkgFhNG.lMEtTROEQbki" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 7, 23, 44, 19, 743, DateTimeKind.Utc).AddTicks(7400), new DateTime(2025, 10, 7, 23, 44, 19, 743, DateTimeKind.Utc).AddTicks(7400), "$2a$11$gzyfS6OYvrwkK2s9LWN8z.9dDw1bVlLRKeEJZRlfpl9xmVOum4ES." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EyeColor",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "LastPhotoUpdate",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "NextPhotoReminder",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "PhotoUpdateReminderCount",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "PhotoUpdateReminderSent",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Runners");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 5, 3, 12, 3, 675, DateTimeKind.Utc).AddTicks(2760), new DateTime(2025, 10, 5, 3, 12, 3, 675, DateTimeKind.Utc).AddTicks(2760), "$2a$11$8cGGRvh4E5nosqaaYfAXp.p.7D2AdiZZWrT1WaIAltz7GFANBUGpK" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 5, 3, 12, 3, 793, DateTimeKind.Utc).AddTicks(2260), new DateTime(2025, 10, 5, 3, 12, 3, 793, DateTimeKind.Utc).AddTicks(2260), "$2a$11$ezlrIIirdJjA4/fVMZY5iuPvgV6iuqcUzQheCnqkZmjooBnKDWBoK" });
        }
    }
}
