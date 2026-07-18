Imports System.Configuration
Imports System.IdentityModel.Tokens.Jwt
Imports System.Net
Imports System.Security.Claims
Imports System.Security.Cryptography
Imports Microsoft.IdentityModel.Tokens

Public Class AuthService
    Implements IAuthService

    Private ReadOnly _userRepo As IUserRepository
    Private ReadOnly _refreshTokenRepo As IRefreshTokenRepository

    Public Sub New(userRepo As IUserRepository, refreshTokenRepo As IRefreshTokenRepository)
        _userRepo = userRepo
        _refreshTokenRepo = refreshTokenRepo
    End Sub

    Public Sub Register(req As Register) Implements IAuthService.Register

        If _userRepo.GetByEmail(req.Email) IsNot Nothing Then
            Throw ApiException.Conflict("Email is already in use.")
        End If

        Dim user As New User With {
            .Id = Guid.NewGuid(),
            .Email = req.Email.Trim().ToLower(),
            .PasswordHash = PasswordHelper.HashPassword(req.Password),
            .IsActive = True,
            .CreatedAt = DateTime.UtcNow
        }

        _userRepo.Create(user)

    End Sub

    Public Function Login(req As Login) As LoginResponse Implements IAuthService.Login

        Dim user = _userRepo.GetByEmail(req.Email?.Trim().ToLower())

        If user Is Nothing Then
            Throw ApiException.Unauthorized("Invalid email or password.")
        End If

        If Not user.IsActive Then
            Throw ApiException.Forbidden("Account has been deactivated.")
        End If

        If Not PasswordHelper.VerifyPassword(req.Password, user.PasswordHash) Then
            Throw ApiException.Unauthorized("Invalid email or password.")
        End If

        Dim oldToken = _refreshTokenRepo.GetActiveTokenByUserId(user.Id)
        If oldToken IsNot Nothing Then
            oldToken.RevokedAt = DateTime.UtcNow
        End If

        Dim accessToken = GenerateAccessToken(user)
        Dim refreshToken = GenerateRefreshToken()

        _refreshTokenRepo.Add(New RefreshToken With {
            .Id = Guid.NewGuid(),
            .UserId = user.Id,
            .Token = refreshToken,
            .CreatedAt = DateTime.UtcNow,
            .ExpiresAt = DateTime.UtcNow.AddDays(7)
        })
        _refreshTokenRepo.Save()

        Return New LoginResponse With {
            .AccessToken = accessToken,
            .RefreshToken = refreshToken
        }

    End Function

    Public Function SearchUsers(keyword As String) As List(Of UserSearchResponse) _
        Implements IAuthService.SearchUsers

        If String.IsNullOrWhiteSpace(keyword) Then
            Return New List(Of UserSearchResponse)()
        End If

        Return _userRepo.SearchUsers(keyword.Trim())

    End Function

    Public Function Refresh(refreshToken As String) As LoginResponse _
        Implements IAuthService.Refresh

        Dim token = _refreshTokenRepo.GetByToken(refreshToken)

        If token Is Nothing Then
            Throw ApiException.Unauthorized("Invalid refresh token.")
        End If

        If token.RevokedAt.HasValue Then
            Throw ApiException.Unauthorized("Refresh token has been revoked. Please sign in again.")
        End If

        If token.ExpiresAt < DateTime.UtcNow Then
            Throw ApiException.Unauthorized("Refresh token has expired. Please sign in again.")
        End If

        Dim user = _userRepo.GetById(token.UserId)

        If user Is Nothing OrElse Not user.IsActive Then
            Throw ApiException.Forbidden("Account is no longer active.")
        End If

        Return New LoginResponse With {
            .AccessToken = GenerateAccessToken(user)
        }

    End Function

    Public Sub Logout(refreshToken As String) Implements IAuthService.Logout

        Dim token = _refreshTokenRepo.GetByToken(refreshToken)
        If token Is Nothing Then Exit Sub

        token.RevokedAt = DateTime.UtcNow
        _refreshTokenRepo.Save()

    End Sub

    Private Function GenerateAccessToken(user As User) As String

        Dim key = New SymmetricSecurityKey(
            Text.Encoding.UTF8.GetBytes(
                ConfigurationManager.AppSettings("Jwt:Key")))

        Dim expireMinutes = Integer.Parse(
            ConfigurationManager.AppSettings("Jwt:ExpireMinutes"))

        Dim token = New JwtSecurityToken(
            issuer:=ConfigurationManager.AppSettings("Jwt:Issuer"),
            audience:=ConfigurationManager.AppSettings("Jwt:Audience"),
            claims:=New List(Of Claim) From {
                New Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                New Claim(ClaimTypes.Email, user.Email)
            },
            expires:=DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials:=New SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        )

        Return New JwtSecurityTokenHandler().WriteToken(token)

    End Function

    Private Function GenerateRefreshToken() As String
        Dim bytes(63) As Byte
        Using rng As New RNGCryptoServiceProvider()
            rng.GetBytes(bytes)
        End Using
        Return Base64UrlEncoder.Encode(bytes)
    End Function

End Class
