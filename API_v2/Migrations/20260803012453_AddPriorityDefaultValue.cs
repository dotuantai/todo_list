using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Tasks",
                type: "text",
                nullable: false,
                defaultValue: "Medium",
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Tasks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Medium");
        }
    }
}
