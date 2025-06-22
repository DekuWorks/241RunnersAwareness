using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _241RunnersAwareness.BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdHeightAndWeightFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmergencyContacts_Individuals_IndividualID",
                table: "EmergencyContacts");

            migrationBuilder.RenameColumn(
                name: "IndividualID",
                table: "Individuals",
                newName: "IndividualId");

            migrationBuilder.RenameColumn(
                name: "IndividualID",
                table: "EmergencyContacts",
                newName: "IndividualId");

            migrationBuilder.RenameIndex(
                name: "IX_EmergencyContacts_IndividualID",
                table: "EmergencyContacts",
                newName: "IX_EmergencyContacts_IndividualId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmergencyContacts_Individuals_IndividualId",
                table: "EmergencyContacts",
                column: "IndividualId",
                principalTable: "Individuals",
                principalColumn: "IndividualId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmergencyContacts_Individuals_IndividualId",
                table: "EmergencyContacts");

            migrationBuilder.RenameColumn(
                name: "IndividualId",
                table: "Individuals",
                newName: "IndividualID");

            migrationBuilder.RenameColumn(
                name: "IndividualId",
                table: "EmergencyContacts",
                newName: "IndividualID");

            migrationBuilder.RenameIndex(
                name: "IX_EmergencyContacts_IndividualId",
                table: "EmergencyContacts",
                newName: "IX_EmergencyContacts_IndividualID");

            migrationBuilder.AddForeignKey(
                name: "FK_EmergencyContacts_Individuals_IndividualID",
                table: "EmergencyContacts",
                column: "IndividualID",
                principalTable: "Individuals",
                principalColumn: "IndividualID");
        }
    }
}
