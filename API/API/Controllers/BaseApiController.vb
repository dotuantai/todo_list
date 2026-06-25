Imports System.Net
Imports System.Net.Http
Imports System.Security.Claims
Imports System.Web.Http

Public MustInherit Class BaseApiController
    Inherits ApiController

    Protected ReadOnly Property CurrentUserId As Guid
        Get
            Dim identity = TryCast(User.Identity, ClaimsIdentity)
            Dim claim = identity?.FindFirst(ClaimTypes.NameIdentifier)

            If claim Is Nothing Then
                Throw ApiException.Unauthorized("Không xác định được người dùng từ token.")
            End If

            Return Guid.Parse(claim.Value)
        End Get
    End Property

    Protected Function Execute(action As Action) As IHttpActionResult
        Try
            action()
            Return Ok(New ApiResponse(Of Object)(True, "Thành công", Nothing))
        Catch ex As ApiException
            Return BuildError(ex)
        End Try
    End Function

    Protected Function Execute(Of T)(action As Func(Of T)) As IHttpActionResult
        Try
            Dim result = action()
            Return Ok(New ApiResponse(Of T)(True, "Thành công", result))
        Catch ex As ApiException
            Return BuildError(ex)
        End Try
    End Function

    Private Function BuildError(ex As ApiException) As IHttpActionResult
        Dim body = New ApiResponse(Of Object)(False, ex.Message, Nothing)

        Select Case ex.StatusCode
            Case Net.HttpStatusCode.BadRequest
                Return Content(Net.HttpStatusCode.BadRequest, body)
            Case Net.HttpStatusCode.Unauthorized
                Return Content(Net.HttpStatusCode.Unauthorized, body)
            Case Net.HttpStatusCode.Forbidden
                Return Content(Net.HttpStatusCode.Forbidden, body)
            Case Net.HttpStatusCode.NotFound
                Return Content(Net.HttpStatusCode.NotFound, body)
            Case Net.HttpStatusCode.Conflict
                Return Content(Net.HttpStatusCode.Conflict, body)
            Case Else
                Return Content(Net.HttpStatusCode.InternalServerError, body)
        End Select
    End Function

End Class

Public Class ApiResponse(Of T)

    Public Property Success As Boolean
    Public Property Message As String
    Public Property Data As T

    Public Sub New(success As Boolean, message As String, data As T)
        Me.Success = success
        Me.Message = message
        Me.Data = data
    End Sub

End Class
