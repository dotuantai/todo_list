Public Interface IUserRepository

    Function GetByEmail(Email As String) As User

    Function Exists(Email As String) As Boolean

    Sub Create(user As User)

End Interface