Imports System.Data.Entity.ModelConfiguration

Public Class UserConfiguration
    Inherits EntityTypeConfiguration(Of User)

    Public Sub New()

        ToTable("users")

        HasKey(Function(x) x.Id)

        [Property](Function(x) x.Email) _
            .HasColumnName("email") _
            .IsRequired() _
            .HasMaxLength(50)

        [Property](Function(x) x.PasswordHash) _
            .HasColumnName("password_hash") _
            .IsRequired()

        [Property](Function(x) x.IsActive) _
            .HasColumnName("is_active")

        [Property](Function(x) x.CreatedAt) _
            .HasColumnName("created_at")

    End Sub

End Class