Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Web.Http

<RoutePrefix("api/auth")>
Public Class AuthController
    Inherits BaseApiController

    Private ReadOnly _authService As IAuthService

    Public Sub New(authService As IAuthService)
        _authService = authService
    End Sub

    <HttpPost>
    <Route("register")>
    <AllowAnonymous>
    Public Function Register(<FromBody> req As Register) As IHttpActionResult

        If req Is Nothing Then
            Return Content(HttpStatusCode.BadRequest,
                New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
        End If

        If Not ModelState.IsValid Then
            Dim errors = ModelState.Values.
                SelectMany(Function(v) v.Errors).
                Select(Function(e) e.ErrorMessage)

            Return Content(HttpStatusCode.BadRequest,
                New ApiResponse(Of Object)(False, String.Join(" | ", errors), Nothing))
        End If

        Return Execute(Function()
                           _authService.Register(req)
                           Return "Đăng ký thành công."
                       End Function)

    End Function

    <HttpPost>
    <Route("login")>
    <AllowAnonymous>
    Public Function Login(<FromBody> req As Login) As IHttpActionResult

        If req Is Nothing OrElse
           String.IsNullOrWhiteSpace(req.Email) OrElse
           String.IsNullOrWhiteSpace(req.Password) Then

            Return Content(HttpStatusCode.BadRequest,
                New ApiResponse(Of Object)(False, "Email và Password không được để trống.", Nothing))
        End If

        Try
            Dim result = _authService.Login(req)

            Dim cookie As New HttpCookie("refreshToken", result.RefreshToken) With {
                .HttpOnly = True,
                .Secure = True,
                .Path = "/",
                .SameSite = SameSiteMode.None,
                .Expires = DateTime.UtcNow.AddDays(7)
            }
            HttpContext.Current.Response.Cookies.Add(cookie)

            Return Ok(New ApiResponse(Of Object)(True, "Đăng nhập thành công.",
                New With {.AccessToken = result.AccessToken}))

        Catch ex As ApiException
            Return Content(ex.StatusCode,
                New ApiResponse(Of Object)(False, ex.Message, Nothing))
        End Try

    End Function

    <HttpGet>
    <Route("search")>
    Public Function SearchUsers(q As String) As IHttpActionResult
        Return Execute(Function() _authService.SearchUsers(q))
    End Function

    <HttpPost>
    <Route("refresh")>
    <AllowAnonymous>
    Public Function Refresh() As IHttpActionResult

        Dim cookie = Request.Headers.GetCookies("refreshToken").FirstOrDefault()
        Dim refreshToken = cookie?.Cookies.FirstOrDefault()?.Value

        If String.IsNullOrEmpty(refreshToken) Then
            Return Content(HttpStatusCode.BadRequest,
                New ApiResponse(Of Object)(False, "Không tìm thấy refresh token.", Nothing))
        End If

        Try
            Dim result = _authService.Refresh(refreshToken)
            Return Ok(New ApiResponse(Of Object)(True, "Làm mới token thành công.",
                New With {.AccessToken = result.AccessToken}))

        Catch ex As ApiException
            Return Content(ex.StatusCode,
                New ApiResponse(Of Object)(False, ex.Message, Nothing))
        End Try

    End Function

    <HttpPost>
    <Route("logout")>
    Public Function Logout() As IHttpActionResult

        Dim cookies = Request.Headers.GetCookies("refreshToken")
        Dim refreshToken As String = cookies?.FirstOrDefault()?.Cookies.FirstOrDefault()?.Value

        If Not String.IsNullOrEmpty(refreshToken) Then
            _authService.Logout(refreshToken)
        End If

        Dim response = Request.CreateResponse(HttpStatusCode.OK,
            New ApiResponse(Of Object)(True, "Đăng xuất thành công.", Nothing))

        Dim expiredCookie As New CookieHeaderValue("refreshToken", "") With {
            .HttpOnly = True,
            .Secure = True,
            .Path = "/",
            .Expires = DateTimeOffset.UtcNow.AddDays(-1)
        }
        response.Headers.AddCookies(New CookieHeaderValue() {expiredCookie})

        Return ResponseMessage(response)

    End Function

End Class
