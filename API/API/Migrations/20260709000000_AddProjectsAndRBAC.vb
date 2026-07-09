Imports System
Imports System.Data.Entity.Migrations

Namespace Migrations
    Public Partial Class AddProjectsAndRBAC
        Inherits DbMigration

        Public Overrides Sub Up()
            ' Create table projects
            CreateTable(
                "dbo.projects",
                Function(c) New With {
                    .Id = c.Guid(nullable := False),
                    .name = c.String(nullable := False, maxLength := 200),
                    .description = c.String(nullable := True, maxLength := 1000),
                    .owner_id = c.Guid(nullable := False),
                    .created_at = c.DateTime(nullable := False),
                    .updated_at = c.DateTime(nullable := False)
                }) _
                .PrimaryKey(Function(t) t.Id) _
                .ForeignKey("dbo.users", Function(t) t.owner_id, cascadeDelete := False)

            ' Create table project_members
            CreateTable(
                "dbo.project_members",
                Function(c) New With {
                    .Id = c.Guid(nullable := False),
                    .project_id = c.Guid(nullable := False),
                    .user_id = c.Guid(nullable := False),
                    .role = c.String(nullable := False, maxLength := 50),
                    .joined_at = c.DateTime(nullable := False)
                }) _
                .PrimaryKey(Function(t) t.Id) _
                .ForeignKey("dbo.projects", Function(t) t.project_id, cascadeDelete := True) _
                .ForeignKey("dbo.users", Function(t) t.user_id, cascadeDelete := False)

            ' Add column project_id to Tasks table (optional for backward compatibility)
            AddColumn("dbo.Tasks", "project_id", Function(c) c.Guid(nullable := True))
            
            ' Create foreign key constraint for project_id on Tasks
            AddForeignKey("dbo.Tasks", "project_id", "dbo.projects", "Id", cascadeDelete := True)
        End Sub

        Public Overrides Sub Down()
            DropForeignKey("dbo.Tasks", "project_id", "dbo.projects")
            DropColumn("dbo.Tasks", "project_id")

            DropForeignKey("dbo.project_members", "user_id", "dbo.users")
            DropForeignKey("dbo.project_members", "project_id", "dbo.projects")
            DropTable("dbo.project_members")

            DropForeignKey("dbo.projects", "owner_id", "dbo.users")
            DropTable("dbo.projects")
        End Sub
    End Class
End Namespace
