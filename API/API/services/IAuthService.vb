Public Interface IAuthService
    Function Register(req As Register) As String
    Function Login(req As Login) As LoginResponse

    Function SearchUsers(keyword As String) As List(Of UserSearchResponse)
    Function Refresh(refreshToken As String) As LoginResponse
    Sub Logout(refreshToken As String)
End Interface