using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "LastSeenLongitude",
                table: "Cases",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LastSeenLatitude",
                table: "Cases",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "LastSeenLongitude",
                table: "Cases",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LastSeenLatitude",
                table: "Cases",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

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
        }
    }
}
