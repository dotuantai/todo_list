Public Class RefreshToken

    Public Property Id As Guid

    Public Property UserId As Guid

    Public Property Token As String

    Public Property CreatedAt As DateTime

    Public Property ExpiresAt As DateTime

    Public Property RevokedAt As Nullable(Of DateTime)

    Public Overridable Property User As User

End Class