Imports System.Data.Entity.Validation
Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports System.Security.Cryptography
Imports System.Web.Helpers
Imports Microsoft.IdentityModel.Tokens
Public Class AuthService
    Implements IAuthService

    Private ReadOnly _userRepo As IUserRepository
    Private ReadOnly _refreshTokenRepo As IRefreshTokenRepository

    Public Sub New(userRepo As IUserRepository, refreshTokenRepo As IRefreshTokenRepository)

        _userRepo = userRepo
        _refreshTokenRepo = refreshTokenRepo

    End Sub

    Public Function Register(req As Register) As String Implements IAuthService.Register

        If _userRepo.Exists(req.Email) Then
            Return Nothing
        End If

        Dim user As New User With {
            .Id = Guid.NewGuid(),
            .Email = req.Email,
            .PasswordHash = PasswordHelper.HashPassword(req.Password),
            .IsActive = True,
            .CreatedAt = DateTime.UtcNow
        }

        _userRepo.Create(user)

        Return "Đăng ký thành công"

    End Function

    Public Function Login(req As Login) As LoginResponse Implements IAuthService.Login
        Try
            If req Is Nothing Then
                Return Nothing
            End If
            Dim userExit = _userRepo.Exists(req.Email)
            If Not userExit Then Return Nothing
            Dim user = _userRepo.GetByEmail(req.Email)

            If user Is Nothing OrElse Not user.IsActive Then
                Return Nothing
            End If
            If Not PasswordHelper.VerifyPassword(req.Password, user.PasswordHash) Then Return Nothing


            Dim accessToken = GenerateToken(user)

            Dim refreshToken = GenerateRefreshToken()
            'Return GenerateToken(user)

            Dim oldToken =
            _refreshTokenRepo.GetActiveTokenByUserId(user.Id)

            If oldToken IsNot Nothing Then

                oldToken.RevokedAt = DateTime.UtcNow

            End If

            Dim refreshEntity As New RefreshToken With {
            .Id = Guid.NewGuid(),
            .UserId = user.Id,
            .Token = refreshToken,
            .CreatedAt = DateTime.UtcNow,
            .ExpiresAt = DateTime.UtcNow.AddDays(30)
            }

            _refreshTokenRepo.Add(refreshEntity)

            _refreshTokenRepo.Save()
            Return New LoginResponse With {
                .AccessToken = accessToken,
                .RefreshToken = refreshToken
            }
        Catch ex As DbEntityValidationException

            For Each eve In ex.EntityValidationErrors

                For Each ve In eve.ValidationErrors

                    Throw New Exception(
                ve.PropertyName &
                " : " &
                ve.ErrorMessage)

                Next

            Next

            Throw

        End Try
    End Function
    Private Function GenerateToken(user As User) As String
        Dim key = New SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                ConfigurationManager.AppSettings("Jwt:Key")))

        Dim claims = New List(Of Claim) From {
            New Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            New Claim(ClaimTypes.Name, user.Email)
        }

        Dim expire = Integer.Parse(
            ConfigurationManager.AppSettings("Jwt:ExpireMinutes"))

        Dim token = New JwtSecurityToken(
            issuer:=ConfigurationManager.AppSettings("Jwt:Issuer"),
            audience:=ConfigurationManager.AppSettings("Jwt:Audience"),
            claims:=claims,
            expires:=DateTime.UtcNow.AddMinutes(expire),
            signingCredentials:=New SigningCredentials(
                key, SecurityAlgorithms.HmacSha256)
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

    Public Function SearchUsers(keyword As String) As List(Of UserSearchResponse) Implements IAuthService.SearchUsers

        If String.IsNullOrWhiteSpace(keyword) Then
            Return New List(Of UserSearchResponse)
        End If

        Return _userRepo.SearchUsers(keyword)

    End Function
    Public Function Refresh(refreshToken As String) As LoginResponse Implements IAuthService.Refresh

        Dim token = _refreshTokenRepo.GetByToken(refreshToken)

        If token Is Nothing Then
            Throw New Exception("Refresh token không hợp lệ")
        End If

        If token.RevokedAt.HasValue Then
            Throw New Exception("Refresh token đã bị thu hồi")
        End If

        If token.ExpiresAt < DateTime.UtcNow Then
            Throw New Exception("Refresh token hết hạn")
        End If

        Dim user = _userRepo.GetById(token.UserId)

        Dim accessToken =
            GenerateToken(user)

        Return New LoginResponse With {
            .AccessToken = accessToken
        }

    End Function

    Public Sub Logout(refreshToken As String) Implements IAuthService.Logout

        Dim token = _refreshTokenRepo.GetByToken(refreshToken)

        If token Is Nothing Then
            Exit Sub
        End If

        token.RevokedAt = DateTime.UtcNow

        _refreshTokenRepo.Save()

    End Sub
End Class