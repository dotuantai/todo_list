Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace Migrations
    Public Partial Class addtablerefreshtoken
        Inherits DbMigration
    
        Public Overrides Sub Up()
            CreateTable(
                "dbo.RefreshTokens",
                Function(c) New With
                    {
                        .Id = c.Guid(nullable := False),
                        .UserId = c.Guid(nullable := False),
                        .Token = c.String(nullable := False, maxLength := 500),
                        .CreatedAt = c.DateTime(nullable := False),
                        .ExpiresAt = c.DateTime(nullable := False),
                        .RevokedAt = c.DateTime()
                    }) _
                .PrimaryKey(Function(t) t.Id) _
                .ForeignKey("dbo.users", Function(t) t.UserId, cascadeDelete := True) _
                .Index(Function(t) t.UserId)
            
        End Sub
        
        Public Overrides Sub Down()
            DropForeignKey("dbo.RefreshTokens", "UserId", "dbo.users")
            DropIndex("dbo.RefreshTokens", New String() { "UserId" })
            DropTable("dbo.RefreshTokens")
        End Sub
    End Class
End Namespace
