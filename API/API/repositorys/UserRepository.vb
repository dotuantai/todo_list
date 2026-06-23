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

    Public Function SearchUsers(keyword As String) As List(Of UserSearchResponse) Implements IUserRepository.SearchUsers

        keyword = keyword.Trim().ToLower()

        Return _db.Users _
            .Where(Function(u) u.IsActive AndAlso
                (u.Email.ToLower().Contains(keyword))) _
            .OrderBy(Function(u) u.Email) _
            .Take(10) _
            .Select(Function(u) New UserSearchResponse With {
                .UserId = u.Id,
                .Email = u.Email
            }) _
            .ToList()

    End Function

End Class