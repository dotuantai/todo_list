Public Interface IUserRepository

    Function GetByEmail(email As String) As User

    Function GetById(id As Guid) As User

    Function Exists(email As String) As Boolean

    Sub Create(user As User)

    Sub Save()

    Function SearchUsers(keyword As String) As List(Of UserSearchResponse)

End Interface
