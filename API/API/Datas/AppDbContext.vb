Imports System.Data.Entity

Public Class AppDbContext
    Inherits DbContext

    Public Sub New()
        MyBase.New("PostgresConnection")
    End Sub

    Public Property Users As DbSet(Of User)
    Public Property Tasks As DbSet(Of TodoTask)
    Public Property TaskAssignments As DbSet(Of TaskAssignment)
    Protected Overrides Sub OnModelCreating(
    modelBuilder As DbModelBuilder)

        modelBuilder.Configurations.Add(New UserConfiguration())
        modelBuilder.Configurations.Add(New TaskConfiguration())
        modelBuilder.Configurations.Add(New TaskAssignmentConfiguration())

        MyBase.OnModelCreating(modelBuilder)

    End Sub
End Class