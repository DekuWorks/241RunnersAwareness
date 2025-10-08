using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRunnerCreatedByUserIdField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
