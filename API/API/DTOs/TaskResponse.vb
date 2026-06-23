Public Class TaskResponse

    Public Property Id As Integer

    Public Property Title As String

    Public Property Description As String

    Public Property CreatedAt As DateTime
    Public Property Deadline As DateTime?

    Public Property Status As String

    Public Property CreatorId As Guid

End Class

Public Class TaskDetailResponse

    Public Property Id As Integer

    Public Property Title As String

    Public Property Description As String

    Public Property CreatedAt As DateTime
    Public Property Deadline As DateTime?

    Public Property Status As String

    Public Property CreatorId As Guid

    Public Property AssignedUsers As List(Of AssignedUserResponse)

End Class

Public Class AssignedUserResponse

    Public Property UserId As Guid

    Public Property Email As String

    Public Property CanView As Boolean

    Public Property CanEdit As Boolean

End Class