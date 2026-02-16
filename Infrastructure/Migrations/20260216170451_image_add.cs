using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class image_add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Confidence",
                table: "ModelOutputs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "ModelInputId",
                table: "ModelOutputs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "ModelOutputs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "ModelInputs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ModelInputs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "ModelInputs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "ModelInputs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ModelInputs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "ModelInputs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ModelOutputs_ModelInputId",
                table: "ModelOutputs",
                column: "ModelInputId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelInputs_PatientId",
                table: "ModelInputs",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelInputs_UploadedAt",
                table: "ModelInputs",
                column: "UploadedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelOutputs_ModelInputs_ModelInputId",
                table: "ModelOutputs",
                column: "ModelInputId",
                principalTable: "ModelInputs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModelOutputs_ModelInputs_ModelInputId",
                table: "ModelOutputs");

            migrationBuilder.DropIndex(
                name: "IX_ModelOutputs_ModelInputId",
                table: "ModelOutputs");

            migrationBuilder.DropIndex(
                name: "IX_ModelInputs_PatientId",
                table: "ModelInputs");

            migrationBuilder.DropIndex(
                name: "IX_ModelInputs_UploadedAt",
                table: "ModelInputs");

            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "ModelOutputs");

            migrationBuilder.DropColumn(
                name: "ModelInputId",
                table: "ModelOutputs");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "ModelOutputs");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "ModelInputs");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ModelInputs");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "ModelInputs");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "ModelInputs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ModelInputs");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "ModelInputs");
        }
    }
}
