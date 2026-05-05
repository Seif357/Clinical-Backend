using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchedulingModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduleSlots_ScheduleId",
                table: "ScheduleSlots");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Doctors_DoctorId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleSlots_Patients_PatientId",
                table: "ScheduleSlots");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleSlots_PatientId_Status",
                table: "ScheduleSlots");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleSlots_ScheduleId_StartTime",
                table: "ScheduleSlots");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_DoctorId",
                table: "Schedules");

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

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_ScheduleId",
                table: "ScheduleSlots",
                column: "ScheduleId");
        }
    }
}
