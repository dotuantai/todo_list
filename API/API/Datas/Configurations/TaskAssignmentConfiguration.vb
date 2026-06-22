Imports System.Data.Entity.ModelConfiguration

Public Class TaskAssignmentConfiguration
    Inherits EntityTypeConfiguration(Of TaskAssignment)

    Public Sub New()

        ToTable("TaskAssignments")

        HasKey(Function(x) New With {
            x.TaskId,
            x.UserId
        })

        [Property](Function(x) x.CanView).
            IsRequired()

        [Property](Function(x) x.CanEdit).
            IsRequired()

        [Property](Function(x) x.AssignedAt).
            IsRequired()

        HasRequired(Function(x) x.Task).
            WithMany(Function(x) x.Assignments).
            HasForeignKey(Function(x) x.TaskId).
            WillCascadeOnDelete(True)

        HasRequired(Function(x) x.User).
            WithMany(Function(x) x.TaskAssignments).
            HasForeignKey(Function(x) x.UserId).
            WillCascadeOnDelete(False)

    End Sub

End Class