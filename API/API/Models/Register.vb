Imports System.ComponentModel.DataAnnotations

Public Class Register
    <Required(ErrorMessage:="Email is required")>
    <EmailAddress(ErrorMessage:="Invalid email address")>
    Public Property Email As String
    <Required(ErrorMessage:="Password is required")>
    <MinLength(6, ErrorMessage:="Password must be at least 6 characters")>
    Public Property Password As String
End Class
