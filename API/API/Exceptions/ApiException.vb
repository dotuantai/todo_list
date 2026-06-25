Imports System.Net

Public Class ApiException
    Inherits Exception

    Public ReadOnly StatusCode As HttpStatusCode

    Public Sub New(statusCode As HttpStatusCode, message As String)
        MyBase.New(message)
        Me.StatusCode = statusCode
    End Sub

    Public Shared Function BadRequest(message As String) As ApiException
        Return New ApiException(HttpStatusCode.BadRequest, message)
    End Function

    Public Shared Function Unauthorized(message As String) As ApiException
        Return New ApiException(HttpStatusCode.Unauthorized, message)
    End Function

    Public Shared Function Forbidden(message As String) As ApiException
        Return New ApiException(HttpStatusCode.Forbidden, message)
    End Function

    Public Shared Function NotFound(message As String) As ApiException
        Return New ApiException(HttpStatusCode.NotFound, message)
    End Function

    Public Shared Function Conflict(message As String) As ApiException
        Return New ApiException(HttpStatusCode.Conflict, message)
    End Function

End Class
