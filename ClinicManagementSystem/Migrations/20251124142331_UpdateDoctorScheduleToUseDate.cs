using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDoctorScheduleToUseDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "DoctorSchedules");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "DoctorSchedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "DoctorSchedules");

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeek",
                table: "DoctorSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
