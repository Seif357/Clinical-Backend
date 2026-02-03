using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class seprate_users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentRequests_DoctorProfiles_DoctorProfileId",
                table: "AppointmentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentRequests_PatientProfiles_PatientProfileId",
                table: "AppointmentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_DoctorProfiles_DoctorProfileId",
                table: "DoctorRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_PatientProfiles_PatientProfileId",
                table: "DoctorRequest");

            migrationBuilder.DropTable(
                name: "PatientProfiles");

            migrationBuilder.DropTable(
                name: "DoctorProfiles");

            migrationBuilder.AddColumn<string>(
                name: "CertificationNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DoctorProfileId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalRecord",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientStatus",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DoctorProfileId",
                table: "AspNetUsers",
                column: "DoctorProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentRequests_AspNetUsers_DoctorProfileId",
                table: "AppointmentRequests",
                column: "DoctorProfileId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentRequests_AspNetUsers_PatientProfileId",
                table: "AppointmentRequests",
                column: "PatientProfileId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_DoctorProfileId",
                table: "AspNetUsers",
                column: "DoctorProfileId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_AspNetUsers_DoctorProfileId",
                table: "DoctorRequest",
                column: "DoctorProfileId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_AspNetUsers_PatientProfileId",
                table: "DoctorRequest",
                column: "PatientProfileId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentRequests_AspNetUsers_DoctorProfileId",
                table: "AppointmentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentRequests_AspNetUsers_PatientProfileId",
                table: "AppointmentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_DoctorProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_AspNetUsers_DoctorProfileId",
                table: "DoctorRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_AspNetUsers_PatientProfileId",
                table: "DoctorRequest");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DoctorProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CertificationNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DoctorProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastSyncAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MedicalRecord",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PatientStatus",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "DoctorProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorProfiles_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DoctorProfileId = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MedicalHistory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientProfiles_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientProfiles_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_DoctorProfileId",
                table: "PatientProfiles",
                column: "DoctorProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentRequests_DoctorProfiles_DoctorProfileId",
                table: "AppointmentRequests",
                column: "DoctorProfileId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentRequests_PatientProfiles_PatientProfileId",
                table: "AppointmentRequests",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_DoctorProfiles_DoctorProfileId",
                table: "DoctorRequest",
                column: "DoctorProfileId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_PatientProfiles_PatientProfileId",
                table: "DoctorRequest",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
