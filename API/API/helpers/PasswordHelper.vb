

Imports Org.BouncyCastle.Crypto.Generators

Public Class PasswordHelper

    Public Shared Function HashPassword(password As String) As String
        Return BCrypt.Net.BCrypt.HashPassword(password)
    End Function

    Public Shared Function VerifyPassword(password As String, hash As String) As Boolean
        Return BCrypt.Net.BCrypt.Verify(password, hash)
    End Function

End Class