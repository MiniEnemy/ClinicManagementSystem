using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppointmentDateTime2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorSchedules_DoctorId",
                table: "DoctorSchedules");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "DoctorSchedules");

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeek",
                table: "DoctorSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSchedules_DoctorId_DayOfWeek",
                table: "DoctorSchedules",
                columns: new[] { "DoctorId", "DayOfWeek" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorSchedules_DoctorId_DayOfWeek",
                table: "DoctorSchedules");

            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "DoctorSchedules");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "DoctorSchedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSchedules_DoctorId",
                table: "DoctorSchedules",
                column: "DoctorId");
        }
    }
}
