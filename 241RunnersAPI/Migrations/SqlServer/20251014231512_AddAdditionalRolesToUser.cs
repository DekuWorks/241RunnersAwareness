using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalRolesToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalRoles",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AdditionalRoles", "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { null, new DateTime(2025, 10, 14, 23, 15, 12, 443, DateTimeKind.Utc).AddTicks(3680), new DateTime(2025, 10, 14, 23, 15, 12, 443, DateTimeKind.Utc).AddTicks(3680), "$2a$11$Tyg7vZXxQuAIvy6rARu7QecaU60TjaGYm7W7/Td1/mWrKmVRz0oWS" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AdditionalRoles", "CreatedAt", "EmailVerifiedAt", "PasswordHash" },
                values: new object[] { null, new DateTime(2025, 10, 14, 23, 15, 12, 559, DateTimeKind.Utc).AddTicks(1840), new DateTime(2025, 10, 14, 23, 15, 12, 559, DateTimeKind.Utc).AddTicks(1840), "$2a$11$rqPTJapvpifCyiFiLNroG.SL2iRV59wHSakde/GXuh97kBwPOGrle" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalRoles",
                table: "Users");

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
        }
    }
}
