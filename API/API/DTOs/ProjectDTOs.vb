Public Class CreateProjectRequest
    Public Property Name As String
    Public Property Description As String
End Class

Public Class UpdateProjectRequest
    Public Property Name As String
    Public Property Description As String
End Class

Public Class ProjectResponse
    Public Property Id As Guid
    Public Property Name As String
    Public Property Description As String
    Public Property OwnerId As Guid
    Public Property OwnerEmail As String
    Public Property CreatedAt As DateTime
    Public Property UpdatedAt As DateTime
    Public Property UserRole As String
End Class

Public Class AddMemberRequest
    Public Property Email As String
    Public Property Role As String
End Class

Public Class UpdateMemberRequest
    Public Property Role As String
End Class

Public Class MemberResponse
    Public Property UserId As Guid
    Public Property Email As String
    Public Property Role As String
    Public Property JoinedAt As DateTime
End Class
