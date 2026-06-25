Imports System.Data.Entity

Public Class UserRepository
    Implements IUserRepository

    Private ReadOnly _db As AppDbContext

    Public Sub New(db As AppDbContext)
        _db = db
    End Sub

    Public Function GetByEmail(email As String) As User Implements IUserRepository.GetByEmail
        Return _db.Users.FirstOrDefault(Function(u) u.Email = email)
    End Function

    Public Function GetById(id As Guid) As User Implements IUserRepository.GetById
        Return _db.Users.Find(id)
    End Function

    Public Function Exists(email As String) As Boolean Implements IUserRepository.Exists
        Return _db.Users.Any(Function(u) u.Email = email)
    End Function

    Public Sub Create(user As User) Implements IUserRepository.Create
        _db.Users.Add(user)
    End Sub

    Public Sub Save() Implements IUserRepository.Save
        _db.SaveChanges()
    End Sub

    Public Function SearchUsers(keyword As String) As List(Of UserSearchResponse) _
        Implements IUserRepository.SearchUsers

        Dim lower = keyword.Trim().ToLower()
        Return _db.Users.
            Where(Function(u) u.IsActive AndAlso u.Email.ToLower().Contains(lower)).
            OrderBy(Function(u) u.Email).
            Take(10).
            Select(Function(u) New UserSearchResponse With {
                .UserId = u.Id,
                .Email = u.Email
            }).
            ToList()

    End Function

End Class
