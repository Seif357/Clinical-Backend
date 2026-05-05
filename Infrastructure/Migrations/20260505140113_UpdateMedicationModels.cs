using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMedicationModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.AddColumn<string>(
                name: "DaysOfWeek",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoctorRequestId",
                table: "ReviewableEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrescribedByDoctorId",
                table: "ReviewableEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReminderTimes",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByDoctorId",
                table: "ReviewableEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "ReviewableEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ReviewableEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedByUserId",
                table: "ReviewableEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_MedicalRecordId",
                table: "ReviewableEntries",
                column: "MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "DoctorRequestId",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "PrescribedByDoctorId",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "ReminderTimes",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "ReviewedByDoctorId",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "ReviewableEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_MedicalRecordId",
                table: "ReviewableEntries",
                column: "MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id");
        }
    }
}
