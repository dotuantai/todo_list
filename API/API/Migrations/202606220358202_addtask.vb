Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace Migrations
    Public Partial Class addtask
        Inherits DbMigration
    
        Public Overrides Sub Up()
            CreateTable(
                "dbo.TaskAssignments",
                Function(c) New With
                    {
                        .TaskId = c.Int(nullable := False),
                        .UserId = c.Guid(nullable := False),
                        .CanView = c.Boolean(nullable := False),
                        .CanEdit = c.Boolean(nullable := False),
                        .AssignedAt = c.DateTime(nullable := False)
                    }) _
                .PrimaryKey(Function(t) New With { t.TaskId, t.UserId }) _
                .ForeignKey("dbo.Tasks", Function(t) t.TaskId, cascadeDelete := True) _
                .ForeignKey("dbo.users", Function(t) t.UserId) _
                .Index(Function(t) t.TaskId) _
                .Index(Function(t) t.UserId)
            
            CreateTable(
                "dbo.Tasks",
                Function(c) New With
                    {
                        .Id = c.Int(nullable := False, identity := True),
                        .Title = c.String(nullable := False, maxLength := 200),
                        .Description = c.String(maxLength := 1000),
                        .CreatedAt = c.DateTime(nullable := False),
                        .CreatorId = c.Guid(nullable := False)
                    }) _
                .PrimaryKey(Function(t) t.Id) _
                .ForeignKey("dbo.users", Function(t) t.CreatorId) _
                .Index(Function(t) t.CreatorId)
            
        End Sub
        
        Public Overrides Sub Down()
            DropForeignKey("dbo.TaskAssignments", "UserId", "dbo.users")
            DropForeignKey("dbo.TaskAssignments", "TaskId", "dbo.Tasks")
            DropForeignKey("dbo.Tasks", "CreatorId", "dbo.users")
            DropIndex("dbo.Tasks", New String() { "CreatorId" })
            DropIndex("dbo.TaskAssignments", New String() { "UserId" })
            DropIndex("dbo.TaskAssignments", New String() { "TaskId" })
            DropTable("dbo.Tasks")
            DropTable("dbo.TaskAssignments")
        End Sub
    End Class
End Namespace
