Public Class TaskService
    Implements ITaskService

    Private ReadOnly _taskRepo As ITaskRepository
    Private ReadOnly _assignRepo As ITaskAssignmentRepository
    Private ReadOnly _projectRepo As IProjectRepository

    Public Sub New(taskRepo As ITaskRepository, assignRepo As ITaskAssignmentRepository, projectRepo As IProjectRepository)
        _taskRepo = taskRepo
        _assignRepo = assignRepo
        _projectRepo = projectRepo
    End Sub

    Public Function CreateTask(req As CreateTaskRequest, creatorId As Guid, projectId As Guid) As String _
        Implements ITaskService.CreateTask

        ' Check project membership & role (Owner or Editor)
        Dim member = _projectRepo.GetMember(projectId, creatorId)
        If member Is Nothing Then
            Throw ApiException.Forbidden("You are not a member of this project.")
        End If
        If Not member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) AndAlso Not member.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase) Then
            Throw ApiException.Forbidden("Only Owners or Editors can create tasks.")
        End If

        If String.IsNullOrWhiteSpace(req.Title) Then
            Throw ApiException.BadRequest("Task title cannot be empty.")
        End If

        Dim status = ParseTaskStatus(req.Status, TaskStatus.ToDo)

        _taskRepo.Add(New TodoTask With {
            .Title = req.Title.Trim(),
            .Description = req.Description?.Trim(),
            .CreatedAt = DateTime.UtcNow,
            .CreatorId = creatorId,
            .Deadline = req.Deadline,
            .Status = status,
            .ProjectId = projectId
        })
        _taskRepo.Save()

        Return "Task created successfully."

    End Function

    Public Function UpdateTask(req As UpdateTaskRequest, currentUserId As Guid) As String _
        Implements ITaskService.UpdateTask

        Dim task = GetTaskOrThrow(req.TaskId)

        If task.ProjectId.HasValue Then
            Dim member = _projectRepo.GetMember(task.ProjectId.Value, currentUserId)
            If member Is Nothing OrElse (Not member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) AndAlso Not member.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase)) Then
                Throw ApiException.Forbidden("You do not have permission to edit tasks in this project.")
            End If
        Else
            If task.CreatorId <> currentUserId Then
                Dim assignment = _assignRepo.GetAssignment(req.TaskId, currentUserId)
                If assignment Is Nothing OrElse Not assignment.CanEdit Then
                    Throw ApiException.Forbidden("You do not have permission to edit this task.")
                End If
            End If
        End If

        If String.IsNullOrWhiteSpace(req.Title) Then
            Throw ApiException.BadRequest("Task title cannot be empty.")
        End If

        task.Title = req.Title.Trim()
        task.Description = req.Description?.Trim()
        task.Deadline = req.Deadline
        task.Status = ParseTaskStatus(req.Status, task.Status)

        _taskRepo.Save()

        Return "Task updated successfully."

    End Function

    Public Function DeleteTask(taskId As Integer, currentUserId As Guid) As String _
        Implements ITaskService.DeleteTask

        Dim task = GetTaskOrThrow(taskId)

        If task.ProjectId.HasValue Then
            Dim member = _projectRepo.GetMember(task.ProjectId.Value, currentUserId)
            If member Is Nothing OrElse (Not member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) AndAlso Not member.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase)) Then
                Throw ApiException.Forbidden("You do not have permission to delete tasks in this project.")
            End If
        Else
            If task.CreatorId <> currentUserId Then
                Throw ApiException.Forbidden("Only the task creator can delete this task.")
            End If
        End If

        _taskRepo.Delete(task)
        _taskRepo.Save()

        Return "Task deleted successfully."

    End Function

    Public Function AssignTask(req As AssignTaskRequest, currentUserId As Guid) As String _
        Implements ITaskService.AssignTask

        Dim task = GetTaskOrThrow(req.TaskId)

        If task.ProjectId.HasValue Then
            Dim member = _projectRepo.GetMember(task.ProjectId.Value, currentUserId)
            If member Is Nothing OrElse (Not member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) AndAlso Not member.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase)) Then
                Throw ApiException.Forbidden("You do not have permission to assign tasks in this project.")
            End If
            
            Dim targetMember = _projectRepo.GetMember(task.ProjectId.Value, req.UserId)
            If targetMember Is Nothing Then
                Throw ApiException.BadRequest("The assignee must be a project member.")
            End If
        Else
            If task.CreatorId <> currentUserId Then
                Throw ApiException.Forbidden("Only the task creator can assign tasks.")
            End If
        End If

        If req.UserId = currentUserId Then
            Throw ApiException.BadRequest("Cannot assign a task to yourself.")
        End If

        If _assignRepo.Exists(req.TaskId, req.UserId) Then
            Throw ApiException.Conflict("This user has already been assigned to this task.")
        End If

        _assignRepo.Add(New TaskAssignment With {
            .TaskId = req.TaskId,
            .UserId = req.UserId,
            .CanView = req.CanView,
            .CanEdit = req.CanEdit,
            .AssignedAt = DateTime.UtcNow
        })
        _assignRepo.Save()

        Return "Task assigned successfully."

    End Function

    Public Function GetProjectTasks(projectId As Guid, userId As Guid) As List(Of TaskDetailResponse) _
        Implements ITaskService.GetProjectTasks

        ' Check project membership
        Dim member = _projectRepo.GetMember(projectId, userId)
        If member Is Nothing Then
            Throw ApiException.Forbidden("You are not a member of this project.")
        End If

        Return _taskRepo.GetTasksByProjectId(projectId).
            Select(AddressOf MapToTaskDetailResponse).
            ToList()

    End Function

    Public Function UpdatePermission(req As AssignTaskRequest, currentUserId As Guid) As String _
        Implements ITaskService.UpdatePermission

        Dim task = GetTaskOrThrow(req.TaskId)

        If task.ProjectId.HasValue Then
            Dim member = _projectRepo.GetMember(task.ProjectId.Value, currentUserId)
            If member Is Nothing OrElse (Not member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) AndAlso Not member.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase)) Then
                Throw ApiException.Forbidden("You do not have permission to update task permissions in this project.")
            End If
        Else
            If task.CreatorId <> currentUserId Then
                Throw ApiException.Forbidden("Only the task creator can update permissions.")
            End If
        End If

        Dim assignment = _assignRepo.GetAssignment(req.TaskId, req.UserId)
        If assignment Is Nothing Then
            Throw ApiException.NotFound("This user has not been assigned to this task.")
        End If

        assignment.CanView = req.CanView
        assignment.CanEdit = req.CanEdit
        _assignRepo.Save()

        Return "Permissions updated successfully."

    End Function

    Public Function RemoveAssignment(req As RemoveAssignmentRequest, currentUserId As Guid) As String _
        Implements ITaskService.RemoveAssignment

        Dim task = GetTaskOrThrow(req.TaskId)

        If task.ProjectId.HasValue Then
            Dim member = _projectRepo.GetMember(task.ProjectId.Value, currentUserId)
            If member Is Nothing OrElse (Not member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) AndAlso Not member.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase)) Then
                Throw ApiException.Forbidden("You do not have permission to revoke assignments in this project.")
            End If
        Else
            If task.CreatorId <> currentUserId Then
                Throw ApiException.Forbidden("Only the task creator can revoke assignments.")
            End If
        End If

        Dim assignment = _assignRepo.GetAssignment(req.TaskId, req.UserId)
        If assignment Is Nothing Then
            Throw ApiException.NotFound("This user has not been assigned to this task.")
        End If

        _assignRepo.Remove(assignment)
        _assignRepo.Save()

        Return "Assignment revoked successfully."

    End Function

    Public Sub ChangeStatus(req As ChangeTaskStatusRequest, currentUserId As Guid) _
        Implements ITaskService.ChangeStatus

        Dim task = GetTaskOrThrow(req.TaskId)

        If task.ProjectId.HasValue Then
            Dim member = _projectRepo.GetMember(task.ProjectId.Value, currentUserId)
            If member Is Nothing OrElse (Not member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) AndAlso Not member.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase)) Then
                Throw ApiException.Forbidden("You do not have permission to change task status in this project.")
            End If
        Else
            If task.CreatorId <> currentUserId Then
                Dim assignment = _assignRepo.GetAssignment(req.TaskId, currentUserId)
                If assignment Is Nothing Then
                    Throw ApiException.Forbidden("You are not assigned to this task.")
                End If
                If Not assignment.CanEdit Then
                    Throw ApiException.Forbidden("You do not have permission to change the status of this task.")
                End If
            End If
        End If

        task.Status = ParseTaskStatus(req.Status, Nothing)
        _taskRepo.Save()

    End Sub

    Private Function GetTaskOrThrow(taskId As Integer) As TodoTask
        Dim task = _taskRepo.GetById(taskId)
        If task Is Nothing Then
            Throw ApiException.NotFound($"Task #{taskId} does not exist.")
        End If
        Return task
    End Function

    Private Function ParseTaskStatus(status As String, defaultValue As TaskStatus) As TaskStatus
        If String.IsNullOrWhiteSpace(status) Then Return defaultValue

        Dim parsed As TaskStatus
        If Not [Enum].TryParse(Of TaskStatus)(status, True, parsed) Then
            Dim validValues = String.Join(", ", [Enum].GetNames(GetType(TaskStatus)))
            Throw ApiException.BadRequest(
                $"Status '{status}' is invalid. Valid values: {validValues}.")
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
