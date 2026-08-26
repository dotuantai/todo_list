using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingDatabaseConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskComments_TaskId",
                table: "TaskComments");

            migrationBuilder.DropIndex(
                name: "IX_ProjectFileVersions_ProjectFileId",
                table: "ProjectFileVersions");

            migrationBuilder.DropIndex(
                name: "IX_ProjectColumns_ProjectId",
                table: "ProjectColumns");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId_ColumnId_Priority",
                table: "Tasks",
                columns: new[] { "ProjectId", "ColumnId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_TaskId_CreatedAt",
                table: "TaskComments",
                columns: new[] { "TaskId", "CreatedAt" });

            // Repair historical duplicate version numbers without deleting files.
            // Stable ordering makes the migration deterministic on every database.
            migrationBuilder.Sql("""
                WITH ranked AS (
                    SELECT "Id",
                           ROW_NUMBER() OVER (
                               PARTITION BY "ProjectFileId"
                               ORDER BY "CreatedAt", "Id") AS "NewVersionNumber"
                    FROM "ProjectFileVersions"
                )
                UPDATE "ProjectFileVersions" AS versions
                SET "VersionNumber" = ranked."NewVersionNumber"
                FROM ranked
                WHERE versions."Id" = ranked."Id";
                """);

            // Normalize existing Kanban orders before enforcing uniqueness.
            migrationBuilder.Sql("""
                WITH ranked AS (
                    SELECT "Id",
                           ROW_NUMBER() OVER (
                               PARTITION BY "ProjectId"
                               ORDER BY "Order", "CreatedAt", "Id") - 1 AS "NewOrder"
                    FROM "ProjectColumns"
                )
                UPDATE "ProjectColumns" AS columns
                SET "Order" = ranked."NewOrder"
                FROM ranked
                WHERE columns."Id" = ranked."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "UQ_ProjectFileVersions_FileId_Version",
                table: "ProjectFileVersions",
                columns: new[] { "ProjectFileId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ProjectColumns_ProjectId_Order",
                table: "ProjectColumns",
                columns: new[] { "ProjectId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_ProjectId_ColumnId_Priority",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_TaskComments_TaskId_CreatedAt",
                table: "TaskComments");

            migrationBuilder.DropIndex(
                name: "UQ_ProjectFileVersions_FileId_Version",
                table: "ProjectFileVersions");

            migrationBuilder.DropIndex(
                name: "UQ_ProjectColumns_ProjectId_Order",
                table: "ProjectColumns");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_TaskId",
                table: "TaskComments",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFileVersions_ProjectFileId",
                table: "ProjectFileVersions",
                column: "ProjectFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectColumns_ProjectId",
                table: "ProjectColumns",
                column: "ProjectId");
        }
    }
}
