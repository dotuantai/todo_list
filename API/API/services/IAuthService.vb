Public Interface IAuthService
    Function Register(req As Register) As String
    Function Login(req As Login) As String
End Interface