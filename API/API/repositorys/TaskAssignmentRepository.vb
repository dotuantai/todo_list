Imports System.Data.Entity
Public Class TaskAssignmentRepository
    Implements ITaskAssignmentRepository

    Private ReadOnly _db As AppDbContext

    Public Sub New(db As AppDbContext)
        _db = db
    End Sub

    Public Function Exists(taskId As Integer, userId As Guid) As Boolean Implements ITaskAssignmentRepository.Exists

        Return _db.TaskAssignments.Any(
            Function(x) x.TaskId = taskId AndAlso
                        x.UserId = userId)

    End Function

    Public Function GetPermission(taskId As Integer, userId As Guid) As TaskAssignment Implements ITaskAssignmentRepository.GetPermission

        Return _db.TaskAssignments.FirstOrDefault(
            Function(x) x.TaskId = taskId AndAlso
                        x.UserId = userId)

    End Function

    Public Function GetAssignedTasks(userId As Guid) As List(Of TaskAssignment) Implements ITaskAssignmentRepository.GetAssignedTasks

        Return _db.TaskAssignments.
        Include(Function(x) x.Task).
        Where(Function(x) x.UserId = userId).
        OrderByDescending(Function(x) x.AssignedAt).
        ToList()

    End Function

    Public Sub Add(assign As TaskAssignment) Implements ITaskAssignmentRepository.Add

        _db.TaskAssignments.Add(assign)

    End Sub

    Public Sub Save() Implements ITaskAssignmentRepository.Save

        _db.SaveChanges()

    End Sub

End Class