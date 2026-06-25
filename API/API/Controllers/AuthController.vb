Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Web.Http
Imports System.Web.Http.Results
Imports Npgsql

<RoutePrefix("api/auth")>
Public Class AuthController
    Inherits ApiController

    Private ReadOnly _authService As IAuthService

    Public Sub New(authService As IAuthService)
        _authService = authService
    End Sub

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

    <HttpPost>
    <Route("login")>
    <AllowAnonymous>
    Public Function Login(<FromBody> req As Login) As IHttpActionResult

        If req Is Nothing OrElse
       String.IsNullOrWhiteSpace(req.Email) OrElse
       String.IsNullOrWhiteSpace(req.Password) Then

            Return BadRequest(
            "Email và Password không được để trống")
        End If

        Dim result = _authService.Login(req)

        If result Is Nothing Then
            Return Unauthorized()
        End If

        Dim cookie As New HttpCookie("refreshToken", result.RefreshToken)

        cookie.HttpOnly = True
        cookie.Secure = True
        cookie.Path = "/"
        cookie.SameSite = SameSiteMode.None
        cookie.Expires = DateTime.UtcNow.AddDays(30)

        HttpContext.Current.Response.Cookies.Add(cookie)

        Return Ok(New With {.AccessToken = result.AccessToken})

    End Function

    <HttpGet>
    <Route("search")>
    Public Function SearchUsers(q As String) As IHttpActionResult

        Dim result = _authService.SearchUsers(q)

        Return Ok(result)

    End Function
    <HttpPost>
    <Route("refresh")>
    <AllowAnonymous>
    Public Function Refresh() As IHttpActionResult
        Dim cookie = Request.Headers.GetCookies("refreshToken").FirstOrDefault()

        If cookie Is Nothing Then
            Return BadRequest("Không tìm thấy refresh token")
        End If

        Dim refreshToken = cookie.Cookies.FirstOrDefault()?.Value

        If String.IsNullOrEmpty(refreshToken) Then
            Return BadRequest("Refresh token không hợp lệ")
        End If

        Dim newAccessToken = _authService.Refresh(refreshToken)

        If newAccessToken Is Nothing Then
            Return Unauthorized()
        End If

        Return Ok(New With {.AccessToken = newAccessToken.AccessToken})
    End Function
    <HttpPost>
    <Route("logout")>
    Public Function Logout() As IHttpActionResult

        Dim refreshToken As String = Nothing

        Dim cookies = Request.Headers.GetCookies("refreshToken")

        If cookies IsNot Nothing AndAlso cookies.Any() Then

            refreshToken =
            cookies.First().
            Cookies.
            First().
            Value

        End If

        If Not String.IsNullOrEmpty(refreshToken) Then

            _authService.Logout(refreshToken)

        End If

        Dim response = Request.CreateResponse(HttpStatusCode.OK)

        Dim expiredCookie As New CookieHeaderValue("refreshToken", "")

        expiredCookie.HttpOnly = True

        expiredCookie.Secure = True

        expiredCookie.Path = "/"

        expiredCookie.Expires = DateTimeOffset.UtcNow.AddDays(-1)

        response.Headers.AddCookies(New CookieHeaderValue() {expiredCookie})

        Return ResponseMessage(response)

    End Function
End Class