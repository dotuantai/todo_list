Imports System.Data.Entity

Public Class AppDbContext
    Inherits DbContext

    Public Sub New()
        MyBase.New("PostgresConnection")
    End Sub

    Public Property Users As DbSet(Of User)
    Protected Overrides Sub OnModelCreating(
    modelBuilder As DbModelBuilder)

        modelBuilder.Configurations.Add(
            New UserConfiguration())

        MyBase.OnModelCreating(modelBuilder)

    End Sub
End Class