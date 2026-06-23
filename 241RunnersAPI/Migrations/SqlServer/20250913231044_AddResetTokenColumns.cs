using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddResetTokenColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpires",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Runners",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Runners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "Cases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Cases",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash", "ResetToken", "ResetTokenExpires" },
                values: new object[] { new DateTime(2025, 9, 13, 23, 10, 44, 607, DateTimeKind.Utc).AddTicks(4280), new DateTime(2025, 9, 13, 23, 10, 44, 607, DateTimeKind.Utc).AddTicks(4280), "$2a$11$wtbtBMklTzAlOZsrwEWkVOysGKczwiZMqXimdTF3YXiFjBoNL9Fia", null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash", "ResetToken", "ResetTokenExpires" },
                values: new object[] { new DateTime(2025, 9, 13, 23, 10, 44, 722, DateTimeKind.Utc).AddTicks(7230), new DateTime(2025, 9, 13, 23, 10, 44, 722, DateTimeKind.Utc).AddTicks(7230), "$2a$11$7Nmy.sEBtVq8ZDcU80p0Tuj12znoGhALebZmjoeWEhKDk7hjcTJFG", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpires",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Cases");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 8, 17, 39, 52, 517, DateTimeKind.Utc).AddTicks(6830), new DateTime(2025, 9, 8, 17, 39, 52, 517, DateTimeKind.Utc).AddTicks(6830), "$2a$11$dpapiU5oDKCjRfxqqiLw7O0RHhPtVgxgbkLElBx.2vxdEsDIOejme" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 8, 17, 39, 52, 637, DateTimeKind.Utc).AddTicks(2400), new DateTime(2025, 9, 8, 17, 39, 52, 637, DateTimeKind.Utc).AddTicks(2400), "$2a$11$GrKUp1Y7G5IPnBNSScZyfu4X92AtEeK6nq8Om9uwS74Q8LKXLQKB." });
        }
    }
}
