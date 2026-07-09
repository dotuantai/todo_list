Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace Migrations
    Public Partial Class addproject
        Inherits DbMigration
    
        Public Overrides Sub Up()
            CreateTable(
                "dbo.project_members",
                Function(c) New With
                    {
                        .Id = c.Guid(nullable := False),
                        .project_id = c.Guid(nullable := False),
                        .user_id = c.Guid(nullable := False),
                        .role = c.String(nullable := False, maxLength := 50),
                        .joined_at = c.DateTime(nullable := False)
                    }) _
                .PrimaryKey(Function(t) t.Id) _
                .ForeignKey("dbo.projects", Function(t) t.project_id, cascadeDelete := True) _
                .ForeignKey("dbo.users", Function(t) t.user_id) _
                .Index(Function(t) t.project_id) _
                .Index(Function(t) t.user_id)
            
            CreateTable(
                "dbo.projects",
                Function(c) New With
                    {
                        .Id = c.Guid(nullable := False),
                        .name = c.String(nullable := False, maxLength := 200),
                        .description = c.String(maxLength := 1000),
                        .owner_id = c.Guid(nullable := False),
                        .created_at = c.DateTime(nullable := False),
                        .updated_at = c.DateTime(nullable := False)
                    }) _
                .PrimaryKey(Function(t) t.Id) _
                .ForeignKey("dbo.users", Function(t) t.owner_id) _
                .Index(Function(t) t.owner_id)
            
            AddColumn("dbo.Tasks", "project_id", Function(c) c.Guid())
            CreateIndex("dbo.Tasks", "project_id")
            AddForeignKey("dbo.Tasks", "project_id", "dbo.projects", "Id", cascadeDelete := True)
        End Sub
        
        Public Overrides Sub Down()
            DropForeignKey("dbo.project_members", "user_id", "dbo.users")
            DropForeignKey("dbo.project_members", "project_id", "dbo.projects")
            DropForeignKey("dbo.projects", "owner_id", "dbo.users")
            DropForeignKey("dbo.Tasks", "project_id", "dbo.projects")
            DropIndex("dbo.Tasks", New String() { "project_id" })
            DropIndex("dbo.projects", New String() { "owner_id" })
            DropIndex("dbo.project_members", New String() { "user_id" })
            DropIndex("dbo.project_members", New String() { "project_id" })
            DropColumn("dbo.Tasks", "project_id")
            DropTable("dbo.projects")
            DropTable("dbo.project_members")
        End Sub
    End Class
End Namespace
