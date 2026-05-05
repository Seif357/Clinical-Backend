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
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Doctors_DoctorId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleSlots_Patients_PatientId",
                table: "ScheduleSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_MedicalRecords_MedicalRecordId",
                table: "Visits");

            migrationBuilder.DropTable(
                name: "Allergies");

            migrationBuilder.DropTable(
                name: "FamilyConditions");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropTable(
                name: "Surgeries");

            migrationBuilder.DropTable(
                name: "TestsTaken");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleSlots_PatientId_Status",
                table: "ScheduleSlots");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleSlots_ScheduleId_StartTime",
                table: "ScheduleSlots");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_DoctorId",
                table: "Schedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Visits",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "BookedAt",
                table: "ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "DoctorNotes",
                table: "ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "PatientNotes",
                table: "ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ScheduleSlots");

            migrationBuilder.RenameTable(
                name: "Visits",
                newName: "ReviewableEntries");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "ScheduleSlots",
                newName: "patientId");

            migrationBuilder.RenameIndex(
                name: "IX_Visits_MedicalRecordId",
                table: "ReviewableEntries",
                newName: "IX_ReviewableEntries_MedicalRecordId");

            migrationBuilder.AlterColumn<string>(
                name: "Treatment_Plan",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReasonForVisit",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DoctorName",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Diagnosis",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "ReviewableEntries",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "Allergy_MedicalRecordId",
                table: "ReviewableEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Allergy_Name",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiagnosisDate",
                table: "ReviewableEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "ReviewableEntries",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Dosage",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "ReviewableEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FamilyCondition_MedicalRecordId",
                table: "ReviewableEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyCondition_Name",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reaction",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Relative",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "ReviewableEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Surgery_MedicalRecordId",
                table: "ReviewableEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Surgery_Name",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TestTaken_Date",
                table: "ReviewableEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TestTaken_MedicalRecordId",
                table: "ReviewableEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestTaken_Name",
                table: "ReviewableEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Visit_Date",
                table: "ReviewableEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Visit_MedicalRecordId",
                table: "ReviewableEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewableEntries",
                table: "ReviewableEntries",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_ScheduleId",
                table: "ScheduleSlots",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewableEntries_Allergy_MedicalRecordId",
                table: "ReviewableEntries",
                column: "Allergy_MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewableEntries_FamilyCondition_MedicalRecordId",
                table: "ReviewableEntries",
                column: "FamilyCondition_MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewableEntries_Surgery_MedicalRecordId",
                table: "ReviewableEntries",
                column: "Surgery_MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewableEntries_TestTaken_MedicalRecordId",
                table: "ReviewableEntries",
                column: "TestTaken_MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewableEntries_Visit_MedicalRecordId",
                table: "ReviewableEntries",
                column: "Visit_MedicalRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_Allergy_MedicalRecordId",
                table: "ReviewableEntries",
                column: "Allergy_MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_FamilyCondition_MedicalRecordId",
                table: "ReviewableEntries",
                column: "FamilyCondition_MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_MedicalRecordId",
                table: "ReviewableEntries",
                column: "MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_Surgery_MedicalRecordId",
                table: "ReviewableEntries",
                column: "Surgery_MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_TestTaken_MedicalRecordId",
                table: "ReviewableEntries",
                column: "TestTaken_MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_Visit_MedicalRecordId",
                table: "ReviewableEntries",
                column: "Visit_MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_Allergy_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_FamilyCondition_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_Surgery_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_TestTaken_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewableEntries_MedicalRecords_Visit_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleSlots_ScheduleId",
                table: "ScheduleSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewableEntries",
                table: "ReviewableEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReviewableEntries_Allergy_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReviewableEntries_FamilyCondition_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReviewableEntries_Surgery_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReviewableEntries_TestTaken_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReviewableEntries_Visit_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Allergy_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Allergy_Name",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "DiagnosisDate",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Dosage",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "FamilyCondition_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "FamilyCondition_Name",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Reaction",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Relative",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Surgery_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Surgery_Name",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "TestTaken_Date",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "TestTaken_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "TestTaken_Name",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Visit_Date",
                table: "ReviewableEntries");

            migrationBuilder.DropColumn(
                name: "Visit_MedicalRecordId",
                table: "ReviewableEntries");

            migrationBuilder.RenameTable(
                name: "ReviewableEntries",
                newName: "Visits");

            migrationBuilder.RenameColumn(
                name: "patientId",
                table: "ScheduleSlots",
                newName: "PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewableEntries_MedicalRecordId",
                table: "Visits",
                newName: "IX_Visits_MedicalRecordId");

            migrationBuilder.AddColumn<DateTime>(
                name: "BookedAt",
                table: "ScheduleSlots",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "ScheduleSlots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorNotes",
                table: "ScheduleSlots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientNotes",
                table: "ScheduleSlots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ScheduleSlots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Treatment_Plan",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReasonForVisit",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DoctorName",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Diagnosis",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Visits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Visits",
                table: "Visits",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Allergies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MedicalRecordId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reaction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Allergies_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FamilyConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiagnosisDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MedicalRecordId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Relative = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyConditions_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Medications",
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

            migrationBuilder.CreateTable(
                name: "Surgeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MedicalRecordId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surgeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Surgeries_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TestsTaken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MedicalRecordId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestsTaken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestsTaken_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_PatientId_Status",
                table: "ScheduleSlots",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_ScheduleId_StartTime",
                table: "ScheduleSlots",
                columns: new[] { "ScheduleId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DoctorId",
                table: "Schedules",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Allergies_MedicalRecordId",
                table: "Allergies",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyConditions_MedicalRecordId",
                table: "FamilyConditions",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescribedMedications_MedicalRecordId",
                table: "Medications",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Surgeries_MedicalRecordId",
                table: "Surgeries",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_TestsTaken_MedicalRecordId",
                table: "TestsTaken",
                column: "MedicalRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Doctors_DoctorId",
                table: "Schedules",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleSlots_Patients_PatientId",
                table: "ScheduleSlots",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_MedicalRecords_MedicalRecordId",
                table: "Visits",
                column: "MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id");
        }
    }
}
