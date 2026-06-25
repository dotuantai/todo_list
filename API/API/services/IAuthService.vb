Public Interface IAuthService
    Sub Register(req As Register)
    Function Login(req As Login) As LoginResponse

    Function SearchUsers(keyword As String) As List(Of UserSearchResponse)
    Function Refresh(refreshToken As String) As LoginResponse
    Sub Logout(refreshToken As String)
End Interface