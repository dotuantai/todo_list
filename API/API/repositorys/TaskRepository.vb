Imports System.Data.Entity

Public Class TaskRepository
    Implements ITaskRepository

    Private ReadOnly _db As AppDbContext

    Public Sub New(db As AppDbContext)
        _db = db
    End Sub

    Public Function GetById(id As Integer) As TodoTask Implements ITaskRepository.GetById

        Return _db.Tasks.FirstOrDefault(
            Function(x) x.Id = id)
    End Function


    Public Function GetCreatedTasks(userId As Guid) As List(Of TodoTask) Implements ITaskRepository.GetCreatedTasks

        Return _db.Tasks.
        Include(Function(x) x.Assignments.Select(Function(a) a.User)).
        Where(Function(x) x.CreatorId = userId).
        OrderByDescending(Function(x) x.CreatedAt).
        ToList()

    End Function

    Public Function GetTasksByProjectId(projectId As Guid) As List(Of TodoTask) Implements ITaskRepository.GetTasksByProjectId

        Return _db.Tasks.
        Include(Function(x) x.Assignments.Select(Function(a) a.User)).
        Where(Function(x) x.ProjectId = projectId).
        OrderByDescending(Function(x) x.CreatedAt).
        ToList()

    End Function
    Public Sub Add(task As TodoTask) Implements ITaskRepository.Add

        _db.Tasks.Add(task)

    End Sub

    Public Sub Update(task As TodoTask) Implements ITaskRepository.Update

        _db.Entry(task).State = EntityState.Modified

    End Sub

    Public Sub Delete(task As TodoTask) Implements ITaskRepository.Delete

        _db.Tasks.Remove(task)

    End Sub

    Public Sub Save() Implements ITaskRepository.Save

        _db.SaveChanges()

    End Sub

End Class