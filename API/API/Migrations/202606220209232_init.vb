Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace Migrations
    Public Partial Class init
        Inherits DbMigration
    
        Public Overrides Sub Up()
            CreateTable(
                "dbo.users",
                Function(c) New With
                    {
                        .Id = c.Guid(nullable := False),
                        .email = c.String(nullable := False, maxLength := 50),
                        .password_hash = c.String(nullable := False),
                        .is_active = c.Boolean(nullable := False),
                        .created_at = c.DateTime(nullable := False)
                    }) _
                .PrimaryKey(Function(t) t.Id)
            
        End Sub
        
        Public Overrides Sub Down()
            DropTable("dbo.users")
        End Sub
    End Class
End Namespace
