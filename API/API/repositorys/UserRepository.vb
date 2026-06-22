Imports System.Data.Entity

Public Class UserRepository
    Implements IUserRepository

    Private ReadOnly _db As AppDbContext

    Public Sub New(
        db As AppDbContext)

        _db = db

    End Sub

    Public Function GetByEmail(
        Email As String) As User _
        Implements IUserRepository.GetByEmail

        Return _db.Users.
            FirstOrDefault(
                Function(x) x.Email = Email)

    End Function

    Public Function Exists(
        Email As String) As Boolean _
        Implements IUserRepository.Exists

        Return _db.Users.
            Any(Function(x) x.Email = Email)

    End Function

    Public Sub Create(
        user As User) _
        Implements IUserRepository.Create

        _db.Users.Add(user)

        _db.SaveChanges()
    End Sub

End Class