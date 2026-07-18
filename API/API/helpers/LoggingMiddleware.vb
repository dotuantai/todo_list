Imports Microsoft.Owin
Imports System.Diagnostics
Imports System.Threading.Tasks

Public Class LoggingMiddleware
    Inherits OwinMiddleware

    Public Sub New(nextMiddleware As OwinMiddleware)
        MyBase.New(nextMiddleware)
    End Sub

    Public Overrides Async Function Invoke(context As IOwinContext) As Task
        Dim sw = Stopwatch.StartNew()
        Dim request = context.Request
        Dim response = context.Response
        Dim ip = GetClientIp(request)

        ' ── Read request body ──────────────────────────
        Dim requestBody = String.Empty
        If request.Body IsNot Nothing Then
            Using reader = New IO.StreamReader(
                request.Body,
                System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks:=False,
                bufferSize:=4096,
                leaveOpen:=True)
                requestBody = Await reader.ReadToEndAsync()
            End Using
            request.Body = New IO.MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(requestBody))
        End If

        ' ✓ Wrap response stream BEFORE calling next 
        Dim originalStream = response.Body
        Dim responseBuffer = New IO.MemoryStream()
        response.Body = responseBuffer

        Dim username = If(context.Authentication?.User?.Identity?.Name, "anonymous")

        LogHelper.Info(
        $">>> REQUEST  {request.Method} {request.Uri.AbsolutePath}",
        New With {
            .user = username,
            .method = request.Method,
            .url = request.Uri.ToString(),
            .IP = ip,
            .body = LogHelper.MaskSensitiveFields(requestBody),
            .userAgent = request.Headers.Get("User-Agent")
        })

        ' ── Call next middleware ──────────────────
        Await MyBase.Next.Invoke(context)

        ' ✓ Read response body AFTER next has finished
        Dim responseBody = String.Empty
        If responseBuffer.Length > 0 Then
            responseBuffer.Seek(0, IO.SeekOrigin.Begin)
            responseBody = Await New IO.StreamReader(
            responseBuffer,
            System.Text.Encoding.UTF8,
            detectEncodingFromByteOrderMarks:=False,
            bufferSize:=4096,
            leaveOpen:=True).ReadToEndAsync()
        End If

        ' ✓ Copy back to original stream so the client receives the response
        responseBuffer.Seek(0, IO.SeekOrigin.Begin)
        Await responseBuffer.CopyToAsync(originalStream)
        response.Body = originalStream

        sw.Stop()
        Dim statusCode = response.StatusCode
        Dim elapsed = sw.ElapsedMilliseconds
        Dim maskedResponse = LogHelper.MaskSensitiveFields(responseBody)

        If statusCode >= 500 Then
            LogHelper.Error(
            $"<<< RESPONSE {request.Method} {request.Uri.AbsolutePath} | {statusCode} | {elapsed}ms",
            data:=New With {
                .user = username,
                .status = statusCode,
                .responseBody = maskedResponse
            })
        ElseIf statusCode >= 400 Then
            LogHelper.Warning(
            $"<<< RESPONSE {request.Method} {request.Uri.AbsolutePath} | {statusCode} | {elapsed}ms",
            New With {
                .user = username,
                .status = statusCode,
                .responseBody = maskedResponse
            })
        Else
            LogHelper.Info(
            $"<<< RESPONSE {request.Method} {request.Uri.AbsolutePath} | {statusCode} | {elapsed}ms",
            New With {
                .user = username,
                .status = statusCode,
                .responseBody = maskedResponse
            })
        End If
    End Function
    Private Function GetClientIp(request As IOwinRequest) As String
        ' When behind a proxy / load balancer
        Dim forwardedFor = request.Headers.Get("X-Forwarded-For")
        If Not String.IsNullOrEmpty(forwardedFor) Then
            Return forwardedFor.Split(","c)(0).Trim()
        End If

        ' Get direct IP
        Dim ip = request.RemoteIpAddress

        ' Convert ::1 to 127.0.0.1 for readability
        If ip = "::1" Then Return "127.0.0.1"

        Return If(ip, "unknown")
    End Function
End Class