Public Class TaskService
    Implements ITaskService
    Private ReadOnly _taskRepo As ITaskRepository
    Private ReadOnly _assignRepo As ITaskAssignmentRepository

    Public Sub New(
        taskRepo As ITaskRepository, assignRepo As ITaskAssignmentRepository)

        _taskRepo = taskRepo
        _assignRepo = assignRepo

    End Sub
    Public Function CreateTask(req As CreateTaskRequest, creatorId As Guid) As String Implements ITaskService.CreateTask

        Dim task As New TodoTask With {
            .Title = req.Title,
            .Description = req.Description,
            .CreatedAt = DateTime.UtcNow,
            .CreatorId = creatorId
        }

        _taskRepo.Add(task)
        _taskRepo.Save()

        Return "Tạo task thành công"

    End Function
    Public Function UpdateTask(req As UpdateTaskRequest, currentUserId As Guid) As String Implements ITaskService.UpdateTask

        Dim task = _taskRepo.GetById(req.TaskId)

        If task Is Nothing Then
            Throw New Exception("Task không tồn tại")
        End If

        If task.CreatorId <> currentUserId Then

            Dim permission =
                _assignRepo.GetPermission(
                    req.TaskId,
                    currentUserId)

            If permission Is Nothing OrElse
               Not permission.CanEdit Then

                Throw New Exception("Không có quyền sửa task")

            End If

        End If

        task.Title = req.Title
        task.Description = req.Description

        _taskRepo.Save()

        Return "Cập nhật thành công"

    End Function
    Public Function DeleteTask(taskId As Integer, currentUserId As Guid) As String Implements ITaskService.DeleteTask

        Dim task = _taskRepo.GetById(taskId)

        If task Is Nothing Then
            Throw New Exception("Task không tồn tại")
        End If

        If task.CreatorId <> currentUserId Then
            Throw New Exception("Chỉ người tạo được xóa task")
        End If

        _taskRepo.Delete(task)

        _taskRepo.Save()

        Return "Xóa task thành công"

    End Function
    Public Function AssignTask(req As AssignTaskRequest, currentUserId As Guid) As String Implements ITaskService.AssignTask

        Dim task = _taskRepo.GetById(req.TaskId)

        If task Is Nothing Then
            Throw New Exception("Task không tồn tại")
        End If

        If task.CreatorId <> currentUserId Then
            Throw New Exception(
                "Chỉ người tạo task mới được giao quyền")
        End If

        If _assignRepo.Exists(req.TaskId, req.UserId) Then

            Throw New Exception(
                "Bạn đã giao task cho user này rồi")

        End If

        Dim assign As New TaskAssignment With {
            .TaskId = req.TaskId,
            .UserId = req.UserId,
            .CanView = req.CanView,
            .CanEdit = req.CanEdit,
            .AssignedAt = DateTime.UtcNow
        }

        _assignRepo.Add(assign)

        _assignRepo.Save()

        Return "Giao task thành công"

    End Function
    Public Function GetMyCreatedTasks(userId As Guid) As List(Of TaskDetailResponse) Implements ITaskService.GetMyCreatedTasks

        Return _taskRepo.GetCreatedTasks(userId).
        Select(Function(t) New TaskDetailResponse With {
            .Id = t.Id,
            .Title = t.Title,
            .Description = t.Description,
            .CreatedAt = t.CreatedAt,
            .CreatorId = t.CreatorId,
            .AssignedUsers = t.Assignments.
                Select(Function(a) New AssignedUserResponse With {
                    .UserId = a.UserId,
                    .Email = a.User.Email,
                    .CanView = a.CanView,
                    .CanEdit = a.CanEdit
                }).
                ToList()
        }).
        ToList()

    End Function
    Public Function GetMyAssignedTasks(userId As Guid) As List(Of TaskResponse) Implements ITaskService.GetMyAssignedTasks

        Return _assignRepo.GetAssignedTasks(userId).
            Select(Function(x) New TaskResponse With {
                .Id = x.Task.Id,
                .Title = x.Task.Title,
                .Description = x.Task.Description,
                .CreatedAt = x.Task.CreatedAt,
                .CreatorId = x.Task.CreatorId
            }).
            ToList()

    End Function
End Class
