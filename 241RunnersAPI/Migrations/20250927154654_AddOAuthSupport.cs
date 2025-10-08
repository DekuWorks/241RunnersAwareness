using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOAuthSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthProvider",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderAccessToken",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderRefreshToken",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProviderTokenExpires",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderUserId",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AuthProvider", "CreatedAt", "EmailVerifiedAt", "PasswordHash", "ProviderAccessToken", "ProviderRefreshToken", "ProviderTokenExpires", "ProviderUserId" },
                values: new object[] { null, new DateTime(2025, 9, 27, 15, 46, 53, 850, DateTimeKind.Utc).AddTicks(5890), new DateTime(2025, 9, 27, 15, 46, 53, 850, DateTimeKind.Utc).AddTicks(5900), "$2a$11$ZkKx1sIsLcaVBZc8SlHwTuTG3V4nCGGGbBYc6tPbTrP31l1Vp7X9a", null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AuthProvider", "CreatedAt", "EmailVerifiedAt", "PasswordHash", "ProviderAccessToken", "ProviderRefreshToken", "ProviderTokenExpires", "ProviderUserId" },
                values: new object[] { null, new DateTime(2025, 9, 27, 15, 46, 53, 961, DateTimeKind.Utc).AddTicks(3090), new DateTime(2025, 9, 27, 15, 46, 53, 961, DateTimeKind.Utc).AddTicks(3090), "$2a$11$QDwuADBosSkIAYfJZ8.G2.3ceZK48U6erbR1viw/XsNHQxTeBRuka", null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthProvider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProviderAccessToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProviderRefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProviderTokenExpires",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProviderUserId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 19, 17, 33, 47, 286, DateTimeKind.Utc).AddTicks(8160), new DateTime(2025, 9, 19, 17, 33, 47, 286, DateTimeKind.Utc).AddTicks(8160), "$2a$11$jeVQpRgyDUfxmROoDfAhrevSm.SDlm4Or2cvukNeU.hbGXkU8RmtW" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 19, 17, 33, 47, 396, DateTimeKind.Utc).AddTicks(3700), new DateTime(2025, 9, 19, 17, 33, 47, 396, DateTimeKind.Utc).AddTicks(3700), "$2a$11$1BSbMpvXw8CJ3B5SHhmBaOcP8m4s8clMHOdkYWDg01SRBjXLS5DAq" });
        }
    }
}
