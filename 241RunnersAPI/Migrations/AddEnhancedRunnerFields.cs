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
            // Add new fields to Runner table
            migrationBuilder.AddColumn<string>(
                name: "Height",
                table: "Runners",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                comment: "Height in format like 5'8\" or 175cm");

            migrationBuilder.AddColumn<string>(
                name: "Weight",
                table: "Runners",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                comment: "Weight in format like 150 lbs or 68 kg");

            migrationBuilder.AddColumn<string>(
                name: "EyeColor",
                table: "Runners",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                comment: "Eye color description");

            // Add photo management fields
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPhotoUpdate",
                table: "Runners",
                type: "datetime2",
                nullable: true,
                comment: "When photos were last updated");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextPhotoReminder",
                table: "Runners",
                type: "datetime2",
                nullable: true,
                comment: "When to send next photo update reminder (6 months from last update)");

            migrationBuilder.AddColumn<bool>(
                name: "PhotoUpdateReminderSent",
                table: "Runners",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Whether photo update reminder has been sent");

            migrationBuilder.AddColumn<int>(
                name: "PhotoUpdateReminderCount",
                table: "Runners",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Number of photo update reminders sent");

            // Add indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_Runners_NextPhotoReminder",
                table: "Runners",
                column: "NextPhotoReminder");

            migrationBuilder.CreateIndex(
                name: "IX_Runners_PhotoUpdateReminderSent",
                table: "Runners",
                column: "PhotoUpdateReminderSent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove indexes
            migrationBuilder.DropIndex(
                name: "IX_Runners_NextPhotoReminder",
                table: "Runners");

            migrationBuilder.DropIndex(
                name: "IX_Runners_PhotoUpdateReminderSent",
                table: "Runners");

            // Remove columns
            migrationBuilder.DropColumn(
                name: "Height",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "EyeColor",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "LastPhotoUpdate",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "NextPhotoReminder",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "PhotoUpdateReminderSent",
                table: "Runners");

            migrationBuilder.DropColumn(
                name: "PhotoUpdateReminderCount",
                table: "Runners");
        }
    }
}
