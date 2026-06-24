Public Class AssignTaskRequest

    Public Property TaskId As Integer

    Public Property UserId As Guid

    Public Property CanView As Boolean

    Public Property CanEdit As Boolean

End Class
Public Class RemoveAssignmentRequest

    Public Property TaskId As Integer

    Public Property UserId As Guid

End Class