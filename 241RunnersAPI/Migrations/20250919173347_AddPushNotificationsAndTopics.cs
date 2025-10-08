using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPushNotificationsAndTopics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FcmToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AppVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TopicsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OsVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AppBuildNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDelivered = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsOpened = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RelatedCaseId = table.Column<int>(type: "int", nullable: true),
                    RelatedUserId = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "normal"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopicSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsSubscribed = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastNotificationSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotificationCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId_Platform",
                table: "Devices",
                columns: new[] { "UserId", "Platform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsSent",
                table: "Notifications",
                column: "IsSent");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Topic",
                table: "Notifications",
                column: "Topic");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicSubscriptions_UserId_Topic",
                table: "TopicSubscriptions",
                columns: new[] { "UserId", "Topic" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "TopicSubscriptions");

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
    }
}
