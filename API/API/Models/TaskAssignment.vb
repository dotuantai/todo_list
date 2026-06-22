Public Class TaskAssignment

    Public Property TaskId As Integer
    Public Property UserId As Guid

    Public Property CanView As Boolean
    Public Property CanEdit As Boolean

    Public Property AssignedAt As DateTime

    Public Overridable Property Task As TodoTask
    Public Overridable Property User As User

End Class