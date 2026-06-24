Public Interface IRefreshTokenRepository
    Function GetByToken(token As String) As RefreshToken

    Function GetActiveTokenByUserId(userId As Guid) As RefreshToken

    Sub Add(token As RefreshToken)

    Sub Save()
End Interface
