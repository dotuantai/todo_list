Imports System.Data.Entity.ModelConfiguration

Public Class RefreshTokenConfiguration
    Inherits EntityTypeConfiguration(Of RefreshToken)

    Public Sub New()

        ToTable("RefreshTokens")

        HasKey(Function(x) x.Id)

        [Property](Function(x) x.Token).
            IsRequired().
            HasMaxLength(500)

        [Property](Function(x) x.CreatedAt).
            IsRequired()

        [Property](Function(x) x.ExpiresAt).
            IsRequired()

        HasRequired(Function(x) x.User).
            WithMany().
            HasForeignKey(Function(x) x.UserId)

    End Sub

End Class