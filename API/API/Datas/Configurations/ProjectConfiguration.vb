Imports System.Data.Entity.ModelConfiguration

Public Class ProjectConfiguration
    Inherits EntityTypeConfiguration(Of Project)

    Public Sub New()
        ToTable("projects")

        HasKey(Function(x) x.Id)

        [Property](Function(x) x.Name) _
            .HasColumnName("name") _
            .IsRequired() _
            .HasMaxLength(200)

        [Property](Function(x) x.Description) _
            .HasColumnName("description") _
            .IsOptional() _
            .HasMaxLength(1000)

        [Property](Function(x) x.OwnerId) _
            .HasColumnName("owner_id") _
            .IsRequired()

        [Property](Function(x) x.CreatedAt) _
            .HasColumnName("created_at") _
            .IsRequired()

        [Property](Function(x) x.UpdatedAt) _
            .HasColumnName("updated_at") _
            .IsRequired()

        ' Setup relationship to the owner User
        HasRequired(Function(x) x.Owner) _
            .WithMany() _
            .HasForeignKey(Function(x) x.OwnerId) _
            .WillCascadeOnDelete(False)
    End Sub
End Class
