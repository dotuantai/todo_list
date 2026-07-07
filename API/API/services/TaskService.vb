Public Class TaskService
    Implements ITaskService

    Private ReadOnly _taskRepo As ITaskRepository
    Private ReadOnly _assignRepo As ITaskAssignmentRepository

    Public Sub New(taskRepo As ITaskRepository, assignRepo As ITaskAssignmentRepository)
        _taskRepo = taskRepo
        _assignRepo = assignRepo
    End Sub

    Public Function CreateTask(req As CreateTaskRequest, creatorId As Guid) As String _
        Implements ITaskService.CreateTask

        If String.IsNullOrWhiteSpace(req.Title) Then
            Throw ApiException.BadRequest("Tiêu đề task không được để trống.")
        End If

        Dim status = ParseTaskStatus(req.Status, TaskStatus.ToDo)

        _taskRepo.Add(New TodoTask With {
            .Title = req.Title.Trim(),
            .Description = req.Description?.Trim(),
            .CreatedAt = DateTime.UtcNow,
            .CreatorId = creatorId,
            .Deadline = req.Deadline,
            .Status = status
        })
        _taskRepo.Save()

        Return "Tạo task thành công."

    End Function

    Public Function UpdateTask(req As UpdateTaskRequest, currentUserId As Guid) As String _
        Implements ITaskService.UpdateTask

        Dim task = GetTaskOrThrow(req.TaskId)

        If task.CreatorId <> currentUserId Then
            Dim assignment = _assignRepo.GetAssignment(req.TaskId, currentUserId)
            If assignment Is Nothing OrElse Not assignment.CanEdit Then
                Throw ApiException.Forbidden("Bạn không có quyền chỉnh sửa task này.")
            End If
        End If

        If String.IsNullOrWhiteSpace(req.Title) Then
            Throw ApiException.BadRequest("Tiêu đề task không được để trống.")
        End If

        task.Title = req.Title.Trim()
        task.Description = req.Description?.Trim()
        task.Deadline = req.Deadline
        task.Status = ParseTaskStatus(req.Status, task.Status)

        _taskRepo.Save()

        Return "Cập nhật task thành công."

    End Function

    Public Function DeleteTask(taskId As Integer, currentUserId As Guid) As String _
        Implements ITaskService.DeleteTask

        Dim task = GetTaskOrThrow(taskId)

        If task.CreatorId <> currentUserId Then
            Throw ApiException.Forbidden("Chỉ người tạo task mới có quyền xóa.")
        End If

        _taskRepo.Delete(task)
        _taskRepo.Save()

        Return "Xóa task thành công."

    End Function

    Public Function AssignTask(req As AssignTaskRequest, currentUserId As Guid) As String _
        Implements ITaskService.AssignTask

        Dim task = GetTaskOrThrow(req.TaskId)

        If task.CreatorId <> currentUserId Then
            Throw ApiException.Forbidden("Chỉ người tạo task mới có quyền giao task.")
        End If

        If req.UserId = currentUserId Then
            Throw ApiException.BadRequest("Không thể giao task cho chính mình.")
        End If

        If _assignRepo.Exists(req.TaskId, req.UserId) Then
            Throw ApiException.Conflict("User này đã được giao task rồi.")
        End If

        _assignRepo.Add(New TaskAssignment With {
            .TaskId = req.TaskId,
            .UserId = req.UserId,
            .CanView = req.CanView,
            .CanEdit = req.CanEdit,
            .AssignedAt = DateTime.UtcNow
        })
        _assignRepo.Save()

        Return "Giao task thành công."

    End Function

    Public Function GetMyCreatedTasks(userId As Guid) As List(Of TaskDetailResponse) _
        Implements ITaskService.GetMyCreatedTasks

        Return _taskRepo.GetCreatedTasks(userId).
            Select(AddressOf MapToTaskDetailResponse).
            ToList()

    End Function

    Public Function GetMyAssignedTasks(userId As Guid) As List(Of TaskResponse) _
        Implements ITaskService.GetMyAssignedTasks

        Return _assignRepo.GetAssignedTasks(userId).
            Select(Function(a) New TaskResponse With {
                .Id = a.Task.Id,
                .Title = a.Task.Title,
                .Description = a.Task.Description,
                .CreatedAt = a.Task.CreatedAt,
                .CreatorId = a.Task.CreatorId,
                .Deadline = a.Task.Deadline,
                .Status = a.Task.Status.ToString(),
                .CanView = a.CanView,
                .CanEdit = a.CanEdit
            }).
            ToList()

    End Function

    Public Function UpdatePermission(req As AssignTaskRequest, currentUserId As Guid) As String _
        Implements ITaskService.UpdatePermission

        Dim task = GetTaskOrThrow(req.TaskId)

        If task.CreatorId <> currentUserId Then
            Throw ApiException.Forbidden("Chỉ người tạo task mới có quyền cập nhật phân quyền.")
        End If

        Dim assignment = _assignRepo.GetAssignment(req.TaskId, req.UserId)
        If assignment Is Nothing Then
            Throw ApiException.NotFound("User này chưa được giao task.")
        End If

        assignment.CanView = req.CanView
        assignment.CanEdit = req.CanEdit
        _assignRepo.Save()

        Return "Cập nhật quyền thành công."

    End Function

    Public Function RemoveAssignment(req As RemoveAssignmentRequest, currentUserId As Guid) As String _
        Implements ITaskService.RemoveAssignment

        Dim task = GetTaskOrThrow(req.TaskId)

        If task.CreatorId <> currentUserId Then
            Throw ApiException.Forbidden("Chỉ người tạo task mới có quyền thu hồi giao việc.")
        End If

        Dim assignment = _assignRepo.GetAssignment(req.TaskId, req.UserId)
        If assignment Is Nothing Then
            Throw ApiException.NotFound("User này chưa được giao task.")
        End If

        _assignRepo.Remove(assignment)
        _assignRepo.Save()

        Return "Thu hồi giao việc thành công."

    End Function

    Public Sub ChangeStatus(req As ChangeTaskStatusRequest, currentUserId As Guid) _
        Implements ITaskService.ChangeStatus

        Dim task = GetTaskOrThrow(req.TaskId)

        If task.CreatorId <> currentUserId Then
            Dim assignment = _assignRepo.GetAssignment(req.TaskId, currentUserId)
            If assignment Is Nothing Then
                Throw ApiException.Forbidden("Bạn không được giao task này.")
            End If
            If Not assignment.CanEdit Then
                Throw ApiException.Forbidden("Bạn không có quyền thay đổi trạng thái task này.")
            End If
        End If

        task.Status = ParseTaskStatus(req.Status, Nothing)
        _taskRepo.Save()

    End Sub

    Private Function GetTaskOrThrow(taskId As Integer) As TodoTask
        Dim task = _taskRepo.GetById(taskId)
        If task Is Nothing Then
            Throw ApiException.NotFound($"Task #{taskId} không tồn tại.")
        End If
        Return task
    End Function

    Private Function ParseTaskStatus(status As String, defaultValue As TaskStatus) As TaskStatus
        If String.IsNullOrWhiteSpace(status) Then Return defaultValue

        Dim parsed As TaskStatus
        If Not [Enum].TryParse(Of TaskStatus)(status, True, parsed) Then
            Dim validValues = String.Join(", ", [Enum].GetNames(GetType(TaskStatus)))
            Throw ApiException.BadRequest(
                $"Trạng thái '{status}' không hợp lệ. Các giá trị hợp lệ: {validValues}.")
        End If
        Return parsed
    End Function

    Private Function MapToTaskDetailResponse(t As TodoTask) As TaskDetailResponse
        Return New TaskDetailResponse With {
            .Id = t.Id,
            .Title = t.Title,
            .Description = t.Description,
            .CreatedAt = t.CreatedAt,
            .CreatorId = t.CreatorId,
            .Deadline = t.Deadline,
            .Status = t.Status.ToString(),
            .AssignedUsers = t.Assignments?.
                Select(Function(a) New AssignedUserResponse With {
                    .UserId = a.UserId,
                    .Email = a.User.Email,
                    .CanView = a.CanView,
                    .CanEdit = a.CanEdit
                }).ToList()
        }
    End Function

    Private Function MapToTaskResponse(t As TodoTask) As TaskResponse
        Return New TaskResponse With {
            .Id = t.Id,
            .Title = t.Title,
            .Description = t.Description,
            .CreatedAt = t.CreatedAt,
            .CreatorId = t.CreatorId,
            .Deadline = t.Deadline,
            .Status = t.Status.ToString()
        }
    End Function

End Class
