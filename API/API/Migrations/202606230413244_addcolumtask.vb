Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace Migrations
    Public Partial Class addcolumtask
        Inherits DbMigration
    
        Public Overrides Sub Up()
            AddColumn("dbo.Tasks", "Deadline", Function(c) c.DateTime())
            AddColumn("dbo.Tasks", "Status", Function(c) c.Int(nullable:=False, defaultValue:=0))
        End Sub
        
        Public Overrides Sub Down()
            DropColumn("dbo.Tasks", "Status")
            DropColumn("dbo.Tasks", "Deadline")
        End Sub
    End Class
End Namespace
