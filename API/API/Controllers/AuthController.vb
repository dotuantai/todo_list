Imports System.Web.Http
Imports Npgsql

<RoutePrefix("api/auth")>
Public Class AuthController
    Inherits ApiController

    Private ReadOnly _authService As IAuthService

    Public Sub New(authService As IAuthService)
        _authService = authService
    End Sub

    ' POST api/auth/register
    <HttpPost>
    <Route("register")>
    <AllowAnonymous>
    Public Function Register(<FromBody> req As Register) As IHttpActionResult

        If req Is Nothing Then
            Return BadRequest("Dữ liệu không hợp lệ")
        End If

        If Not ModelState.IsValid Then
            Dim errors = ModelState.Values.
            SelectMany(Function(v) v.Errors).
            Select(Function(e) e.ErrorMessage)

            Return BadRequest(String.Join(", ", errors))
        End If

        Dim result = _authService.Register(req)

        If result Is Nothing Then
            Return BadRequest("Email đã tồn tại")
        End If

        Return Ok(New With {.message = result})

    End Function

    ' POST api/auth/login
    <HttpPost>
    <Route("login")>
    <AllowAnonymous>
    Public Function Login(<FromBody> req As Login) As IHttpActionResult
        If req Is Nothing OrElse
           String.IsNullOrEmpty(req.Email) OrElse
           String.IsNullOrEmpty(req.Password) Then
            Return BadRequest("Email và Password không được để trống")
        End If

        Dim token = _authService.Login(req)
        If token Is Nothing Then
            Return Unauthorized()
        End If
        Return Ok(New With {.token = token})
    End Function

    ' GET api/auth/profile  — cần token hợp lệ
    <HttpGet>
    <Route("profile")>
    <Authorize>
    Public Function Profile() As IHttpActionResult
        Dim Email = User.Identity.Name
        Return Ok(New With {.Email = Email})
    End Function


    <HttpGet>
    <Route("testdb")>
    <AllowAnonymous>
    Public Function TestDb() As IHttpActionResult

        Try

            Using conn As New NpgsqlConnection(
            "Host=localhost;Port=5432;Database=todo_listt;Username=postgres;Password=123123")

                conn.Open()

                Return Ok("Connected Successfully")

            End Using

        Catch ex As Exception

            Return BadRequest(ex.ToString())

        End Try

    End Function
End Class