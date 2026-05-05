using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMedicalRecordModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByDoctorId",
                table: "Visits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedByUserId",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "TestsTaken",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByDoctorId",
                table: "TestsTaken",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TestsTaken",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedByUserId",
                table: "TestsTaken",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "Surgeries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByDoctorId",
                table: "Surgeries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Surgeries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedByUserId",
                table: "Surgeries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "MedicalRecordId",
                table: "PrescribedMedications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "PrescribedMedications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByDoctorId",
                table: "PrescribedMedications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PrescribedMedications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedByUserId",
                table: "PrescribedMedications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "FamilyConditions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByDoctorId",
                table: "FamilyConditions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "FamilyConditions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedByUserId",
                table: "FamilyConditions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "Allergies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByDoctorId",
                table: "Allergies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Allergies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedByUserId",
                table: "Allergies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_PrescribedMedications_MedicalRecords_MedicalRecordId",
                table: "PrescribedMedications",
                column: "MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrescribedMedications_MedicalRecords_MedicalRecordId",
                table: "PrescribedMedications");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ReviewedByDoctorId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "TestsTaken");

            migrationBuilder.DropColumn(
                name: "ReviewedByDoctorId",
                table: "TestsTaken");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TestsTaken");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "TestsTaken");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "Surgeries");

            migrationBuilder.DropColumn(
                name: "ReviewedByDoctorId",
                table: "Surgeries");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Surgeries");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "Surgeries");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "PrescribedMedications");

            migrationBuilder.DropColumn(
                name: "ReviewedByDoctorId",
                table: "PrescribedMedications");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PrescribedMedications");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "PrescribedMedications");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "FamilyConditions");

            migrationBuilder.DropColumn(
                name: "ReviewedByDoctorId",
                table: "FamilyConditions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FamilyConditions");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "FamilyConditions");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "Allergies");

            migrationBuilder.DropColumn(
                name: "ReviewedByDoctorId",
                table: "Allergies");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Allergies");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "Allergies");

            migrationBuilder.AlterColumn<int>(
                name: "MedicalRecordId",
                table: "PrescribedMedications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_PrescribedMedications_MedicalRecords_MedicalRecordId",
                table: "PrescribedMedications",
                column: "MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id");
        }
    }
}
