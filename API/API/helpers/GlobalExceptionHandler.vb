Imports System.Net
Imports System.Net.Http
Imports System.Web.Http
Imports System.Web.Http.ExceptionHandling
Imports Newtonsoft.Json

Public Class GlobalExceptionHandler
    Inherits ExceptionHandler

    Public Overrides Sub Handle(context As ExceptionHandlerContext)

        Dim ex = context.Exception
        Dim statusCode As HttpStatusCode
        Dim message As String

        If TypeOf ex Is ApiException Then
            Dim apiEx = CType(ex, ApiException)
            statusCode = apiEx.StatusCode
            message = apiEx.Message
        Else
            LogHelper.Error("Unhandled exception", ex)
            statusCode = HttpStatusCode.InternalServerError
            message = "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau."
        End If

        Dim body = JsonConvert.SerializeObject(New With {
            .success = False,
            .message = message,
            .statusCode = CInt(statusCode)
        })

        context.Result = New ErrorActionResult(context.Request, statusCode, body)

    End Sub
    Private Class ErrorActionResult
        Implements IHttpActionResult

        Private ReadOnly _request As HttpRequestMessage
        Private ReadOnly _statusCode As HttpStatusCode
        Private ReadOnly _body As String

        Public Sub New(request As HttpRequestMessage, statusCode As HttpStatusCode, body As String)
            _request = request
            _statusCode = statusCode
            _body = body
        End Sub

        Public Function ExecuteAsync(cancellationToken As Threading.CancellationToken) _
            As Threading.Tasks.Task(Of HttpResponseMessage) _
            Implements IHttpActionResult.ExecuteAsync

            Dim response = New HttpResponseMessage(_statusCode) With {
                .Content = New StringContent(_body, Text.Encoding.UTF8, "application/json"),
                .RequestMessage = _request
            }
            Return Threading.Tasks.Task.FromResult(response)
        End Function
    End Class

End Class
