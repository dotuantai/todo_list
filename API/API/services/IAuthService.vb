Public Interface IAuthService
    Function Register(req As Register) As String
    Function Login(req As Login) As String

    Function SearchUsers(keyword As String) As List(Of UserSearchResponse)
End Interface