Public Class RefreshTokenRepository
    Implements IRefreshTokenRepository

    Private ReadOnly _db As AppDbContext

    Public Sub New(db As AppDbContext)
        _db = db
    End Sub

    Public Function GetByToken(token As String) As RefreshToken _
        Implements IRefreshTokenRepository.GetByToken

        Return _db.RefreshTokens.
            FirstOrDefault(Function(x) x.Token = token)

    End Function

    Public Function GetActiveTokenByUserId(userId As Guid) As RefreshToken _
        Implements IRefreshTokenRepository.GetActiveTokenByUserId

        Return _db.RefreshTokens.FirstOrDefault(
        Function(x) x.UserId = userId AndAlso
                    x.RevokedAt Is Nothing AndAlso
                    x.ExpiresAt > DateTime.UtcNow)

    End Function

    Public Sub Add(token As RefreshToken) _
        Implements IRefreshTokenRepository.Add

        _db.RefreshTokens.Add(token)

    End Sub

    Public Sub Save() _
        Implements IRefreshTokenRepository.Save

        _db.SaveChanges()

    End Sub

End Class