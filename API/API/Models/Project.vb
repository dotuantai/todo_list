Imports System.Collections.Generic

Public Class Project
    Public Property Id As Guid
    Public Property Name As String
    Public Property Description As String
    Public Property OwnerId As Guid
    Public Property CreatedAt As DateTime
    Public Property UpdatedAt As DateTime

    Public Overridable Property Owner As User
    Public Overridable Property Tasks As ICollection(Of TodoTask)
    Public Overridable Property ProjectMembers As ICollection(Of ProjectMember)
End Class
