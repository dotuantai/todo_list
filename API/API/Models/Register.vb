Imports System.ComponentModel.DataAnnotations

Public Class Register
    <Required(ErrorMessage:="Email khong duoc de trong")>
    <EmailAddress(ErrorMessage:="Email khong hop le")>
    Public Property Email As String
    <Required(ErrorMessage:="Mat khau khong duoc de trong")>
    <MinLength(6, ErrorMessage:="Mat khau phai co it nhat 6 ky tu")>
    Public Property Password As String
End Class
