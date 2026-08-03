using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskTimeTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ActualHours",
                table: "Tasks",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EstimatedHours",
                table: "Tasks",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualHours",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Tasks");
        }
    }
}
