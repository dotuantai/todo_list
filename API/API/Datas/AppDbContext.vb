Imports System.Data.Entity

Public Class AppDbContext
    Inherits DbContext

    Public Sub New()
        MyBase.New("PostgresConnection")
    End Sub

    Public Property Users As DbSet(Of User)
    Public Property Tasks As DbSet(Of TodoTask)
    Public Property TaskAssignments As DbSet(Of TaskAssignment)
    Public Property RefreshTokens As DbSet(Of RefreshToken)
    Public Property Projects As DbSet(Of Project)
    Public Property ProjectMembers As DbSet(Of ProjectMember)
    Protected Overrides Sub OnModelCreating(
    modelBuilder As DbModelBuilder)

        modelBuilder.Configurations.Add(New UserConfiguration())
        modelBuilder.Configurations.Add(New TaskConfiguration())
        modelBuilder.Configurations.Add(New TaskAssignmentConfiguration())
        modelBuilder.Configurations.Add(New RefreshTokenConfiguration())
        modelBuilder.Configurations.Add(New ProjectConfiguration())
        modelBuilder.Configurations.Add(New ProjectMemberConfiguration())

        MyBase.OnModelCreating(modelBuilder)

    End Sub
End Class