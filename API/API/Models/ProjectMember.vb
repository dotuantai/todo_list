Public Class ProjectMember
    Public Property Id As Guid
    Public Property ProjectId As Guid
    Public Property UserId As Guid
    Public Property Role As String
    Public Property JoinedAt As DateTime

    Public Overridable Property Project As Project
    Public Overridable Property User As User
End Class
