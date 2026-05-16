using System;
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
            migrationBuilder.DropTable(
                name: "PrescribedMedications");
            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MedicalRecordId = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    PrescribedByDoctorId = table.Column<int>(type: "int", nullable: true),
                    DoctorRequestId = table.Column<int>(type: "int", nullable: true),
                    ReminderTimes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaysOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedByUserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedByDoctorId = table.Column<int>(type: "int", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medications_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medications_MedicalRecordId",
                table: "Medications",
                column: "MedicalRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medications");

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

            migrationBuilder.CreateTable(
                name: "PrescribedMedications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MedicalRecordId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescribedMedications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescribedMedications_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrescribedMedications_MedicalRecordId",
                table: "PrescribedMedications",
                column: "MedicalRecordId");
        }
    }
}
