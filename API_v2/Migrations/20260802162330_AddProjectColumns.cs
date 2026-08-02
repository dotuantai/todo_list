using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Tasks",
                newName: "ColumnId");

            migrationBuilder.CreateTable(
                name: "ProjectColumns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsCompletedStage = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectColumns_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ColumnId",
                table: "Tasks",
                column: "ColumnId");

            // --- CUSTOM SQL MIGRATION START ---
            migrationBuilder.Sql(@"
                DELETE FROM ""Tasks"" WHERE ""ProjectId"" IS NULL;

                DO $$ 
                DECLARE 
                    proj RECORD;
                    col_todo INT;
                    col_inprog INT;
                    col_done INT;
                    col_closed INT;
                BEGIN 
                    FOR proj IN SELECT ""Id"" FROM ""Projects"" LOOP
                        INSERT INTO ""ProjectColumns"" (""ProjectId"", ""Name"", ""Order"", ""IsCompletedStage"", ""CreatedAt"") 
                        VALUES (proj.""Id"", 'To Do', 0, false, NOW()) RETURNING ""Id"" INTO col_todo;
                        
                        INSERT INTO ""ProjectColumns"" (""ProjectId"", ""Name"", ""Order"", ""IsCompletedStage"", ""CreatedAt"") 
                        VALUES (proj.""Id"", 'In Progress', 1, false, NOW()) RETURNING ""Id"" INTO col_inprog;
                        
                        INSERT INTO ""ProjectColumns"" (""ProjectId"", ""Name"", ""Order"", ""IsCompletedStage"", ""CreatedAt"") 
                        VALUES (proj.""Id"", 'Done', 2, true, NOW()) RETURNING ""Id"" INTO col_done;
                        
                        INSERT INTO ""ProjectColumns"" (""ProjectId"", ""Name"", ""Order"", ""IsCompletedStage"", ""CreatedAt"") 
                        VALUES (proj.""Id"", 'Closed', 3, true, NOW()) RETURNING ""Id"" INTO col_closed;

                        UPDATE ""Tasks"" SET ""ColumnId"" = col_todo WHERE ""ProjectId"" = proj.""Id"" AND ""ColumnId"" = 0;
                        UPDATE ""Tasks"" SET ""ColumnId"" = col_inprog WHERE ""ProjectId"" = proj.""Id"" AND ""ColumnId"" = 1;
                        UPDATE ""Tasks"" SET ""ColumnId"" = col_done WHERE ""ProjectId"" = proj.""Id"" AND ""ColumnId"" = 2;
                        UPDATE ""Tasks"" SET ""ColumnId"" = col_closed WHERE ""ProjectId"" = proj.""Id"" AND ""ColumnId"" = 3;
                    END LOOP;
                END $$;
            ");
            // --- CUSTOM SQL MIGRATION END ---

            migrationBuilder.CreateIndex(
                name: "IX_ProjectColumns_ProjectId",
                table: "ProjectColumns",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_ProjectColumns_ColumnId",
                table: "Tasks",
                column: "ColumnId",
                principalTable: "ProjectColumns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_ProjectColumns_ColumnId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "ProjectColumns");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ColumnId",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "ColumnId",
                table: "Tasks",
                newName: "Status");
        }
    }
}
