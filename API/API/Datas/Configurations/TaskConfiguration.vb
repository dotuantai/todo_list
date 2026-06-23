Imports System.Data.Entity.ModelConfiguration

Public Class TaskConfiguration
    Inherits EntityTypeConfiguration(Of TodoTask)

    Public Sub New()


        ToTable("Tasks")

        HasKey(Function(x) x.Id)

        [Property](Function(x) x.Title).
            IsRequired().
            HasMaxLength(200)

        [Property](Function(x) x.Description).
            IsOptional().
            HasMaxLength(1000)

        [Property](Function(x) x.CreatedAt).
            IsRequired()
        [Property](Function(x) x.Deadline).
            IsOptional()

        [Property](Function(x) x.Status).
            IsRequired()
        HasRequired(Function(x) x.Creator).
            WithMany(Function(x) x.CreatedTasks).
            HasForeignKey(Function(x) x.CreatorId).
            WillCascadeOnDelete(False)

    End Sub

End Class