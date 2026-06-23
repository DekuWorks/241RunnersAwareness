using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRunnerShowOnMapAndCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowOnMap",
                table: "Runners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MapLatitude",
                table: "Runners",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MapLongitude",
                table: "Runners",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 9, 19, 22, 15, 613, DateTimeKind.Utc).AddTicks(1940), new DateTime(2026, 3, 9, 19, 22, 15, 613, DateTimeKind.Utc).AddTicks(1940), "$2a$11$0eLmkk5sjXyviDfI/B9hhuwjKk4htf4sl1U6ospEfKO57Eq/Kywam" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 9, 19, 22, 15, 725, DateTimeKind.Utc).AddTicks(500), new DateTime(2026, 3, 9, 19, 22, 15, 725, DateTimeKind.Utc).AddTicks(500), "$2a$11$TmoyfKjksPHWHz3tllA9huaRDo0iG/Dqxi9Ig0ORBw6IBW3U3gfiO" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowOnMap",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "MapLatitude",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "MapLongitude",
                table: "Runners");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 14, 23, 15, 12, 443, DateTimeKind.Utc).AddTicks(3680), new DateTime(2025, 10, 14, 23, 15, 12, 443, DateTimeKind.Utc).AddTicks(3680), "$2a$11$Tyg7vZXxQuAIvy6rARu7QecaU60TjaGYm7W7/Td1/mWrKmVRz0oWS" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 14, 23, 15, 12, 559, DateTimeKind.Utc).AddTicks(1840), new DateTime(2025, 10, 14, 23, 15, 12, 559, DateTimeKind.Utc).AddTicks(1840), "$2a$11$rqPTJapvpifCyiFiLNroG.SL2iRV59wHSakde/GXuh97kBwPOGrle" });
        }
    }
}
