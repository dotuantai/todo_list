Public Class User
    Public Property Id As Guid
    Public Property Email As String
    Public Property PasswordHash As String
    Public Property IsActive As Boolean
    Public Property CreatedAt As DateTime
    Public Overridable Property CreatedTasks As ICollection(Of TodoTask)
    Public Overridable Property TaskAssignments As ICollection(Of TaskAssignment)
    Public Overridable Property ProjectMembers As ICollection(Of ProjectMember)
End Class