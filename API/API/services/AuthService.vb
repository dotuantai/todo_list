Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports System.Web.Helpers
Imports Microsoft.IdentityModel.Tokens
Public Class AuthService
    Implements IAuthService

    Private ReadOnly _userRepo As IUserRepository

    Public Sub New(
        userRepo As IUserRepository)

        _userRepo = userRepo

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

    Public Function Login(req As Login) As String Implements IAuthService.Login
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

            user.PasswordHash = Nothing
            Return GenerateToken(user)
        Catch ex As Exception

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

    Public Function SearchUsers(keyword As String) As List(Of UserSearchResponse) Implements IAuthService.SearchUsers

        If String.IsNullOrWhiteSpace(keyword) Then
            Return New List(Of UserSearchResponse)
        End If

        Return _userRepo.SearchUsers(keyword)

    End Function
End Class