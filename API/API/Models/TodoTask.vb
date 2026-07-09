Imports System.Threading.Tasks

Public Class TodoTask

    Public Property Id As Integer

    Public Property Title As String
    Public Property Description As String

    Public Property CreatedAt As DateTime
    Public Property Deadline As DateTime?

    Public Property Status As TaskStatus


    Public Property CreatorId As Guid

    Public Overridable Property Creator As User

    Public Overridable Property Assignments As ICollection(Of TaskAssignment)

    Public Property ProjectId As Guid?
    Public Overridable Property Project As Project

End Class
Public Enum TaskStatus
    ToDo = 0
    InProgress = 1
    Done = 2
    Closed = 3
End Enum