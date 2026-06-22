Public Class TodoTask

    Public Property Id As Integer

    Public Property Title As String
    Public Property Description As String

    Public Property CreatedAt As DateTime

    Public Property CreatorId As Guid

    Public Overridable Property Creator As User

    Public Overridable Property Assignments As ICollection(Of TaskAssignment)

End Class