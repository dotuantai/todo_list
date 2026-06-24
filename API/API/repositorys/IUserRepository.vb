Public Interface IUserRepository

    Function GetByEmail(Email As String) As User

    Function Exists(Email As String) As Boolean

    Sub Create(user As User)
    Function SearchUsers(keyword As String) As List(Of UserSearchResponse)

    Function GetById(id As Guid) As User
End Interface