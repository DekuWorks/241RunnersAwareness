using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRunnerHeightWeightEyeColorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
