using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddBacklogFileFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentVersion",
                table: "ProjectFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "FolderId",
                table: "ProjectFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ProjectFiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "ProjectFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectFileActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    TargetName = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectFileActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectFileActivities_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectFileActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectFileVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    GoogleDriveFileId = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    ChangeNote = table.Column<string>(type: "text", nullable: true),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectFileVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectFileVersions_ProjectFiles_ProjectFileId",
                        column: x => x.ProjectFileId,
                        principalTable: "ProjectFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectFileVersions_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectFolders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentFolderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GoogleDriveFolderId = table.Column<string>(type: "text", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectFolders_ProjectFolders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "ProjectFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectFolders_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectFolders_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFiles_FolderId",
                table: "ProjectFiles",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFiles_UpdatedById",
                table: "ProjectFiles",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFileActivities_ProjectId",
                table: "ProjectFileActivities",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFileActivities_UserId",
                table: "ProjectFileActivities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFileVersions_ProjectFileId",
                table: "ProjectFileVersions",
                column: "ProjectFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFileVersions_UploadedById",
                table: "ProjectFileVersions",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFolders_CreatedById",
                table: "ProjectFolders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFolders_ParentFolderId",
                table: "ProjectFolders",
                column: "ParentFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFolders_ProjectId",
                table: "ProjectFolders",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectFiles_ProjectFolders_FolderId",
                table: "ProjectFiles",
                column: "FolderId",
                principalTable: "ProjectFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectFiles_Users_UpdatedById",
                table: "ProjectFiles",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectFiles_ProjectFolders_FolderId",
                table: "ProjectFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectFiles_Users_UpdatedById",
                table: "ProjectFiles");

            migrationBuilder.DropTable(
                name: "ProjectFileActivities");

            migrationBuilder.DropTable(
                name: "ProjectFileVersions");

            migrationBuilder.DropTable(
                name: "ProjectFolders");

            migrationBuilder.DropIndex(
                name: "IX_ProjectFiles_FolderId",
                table: "ProjectFiles");

            migrationBuilder.DropIndex(
                name: "IX_ProjectFiles_UpdatedById",
                table: "ProjectFiles");

            migrationBuilder.DropColumn(
                name: "CurrentVersion",
                table: "ProjectFiles");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "ProjectFiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProjectFiles");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "ProjectFiles");
        }
    }
}
