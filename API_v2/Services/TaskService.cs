using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Exceptions;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositorys.IRepositorys;
using API_v2.Services.Interfaces;
using TaskStatusModel = API_v2.Models.TaskStatus;

namespace API_v2.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepo;
        private readonly ITaskAssignmentRepository _assignRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly INotificationService _notificationService;

        public TaskService(
            ITaskRepository taskRepo, 
            ITaskAssignmentRepository assignRepo, 
            IProjectRepository projectRepo,
            INotificationService notificationService)
        {
            _taskRepo = taskRepo;
            _assignRepo = assignRepo;
            _projectRepo = projectRepo;
            _notificationService = notificationService;
        }

        public async Task<string> CreateTaskAsync(CreateTaskRequest req, Guid creatorId, Guid projectId)
        {
            await VerifyOwnerOrManagerAsync(projectId, creatorId, "Only Owners or Managers can create tasks.");

            if (string.IsNullOrWhiteSpace(req.Title))
            {
                throw ApiException.BadRequest("Task title cannot be empty.");
            }

            var status = ParseTaskStatus(req.Status, TaskStatusModel.ToDo);
            var task = new TodoTask
            {
                Title = req.Title.Trim(),
                Description = req.Description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatorId = creatorId,
                Deadline = NormalizeToUtc(req.Deadline),
                Status = status,
                ProjectId = projectId
            };
            _taskRepo.Add(task);
            await _taskRepo.SaveAsync();

            try
            {
                var taskWithDetails = await _taskRepo.GetByIdWithDetailsAsync(task.Id);
                if (taskWithDetails != null)
                {
                    await _notificationService.SendTaskCreatedAsync(projectId, MapToTaskDetailResponse(taskWithDetails));
                }
            }
            catch (Exception)
            {
                // Soft fail on signalr error so it doesn't break API response
            }

            return "Task created successfully.";
        }

        public async Task<string> UpdateTaskAsync(UpdateTaskRequest req, Guid currentUserId)
        {
            var task = await GetTaskOrThrowAsync(req.TaskId);

            if (task.ProjectId.HasValue)
            {
                await VerifyOwnerOrManagerAsync(task.ProjectId.Value, currentUserId, "You do not have permission to edit tasks in this project.");
            }
            else
            {
                if (task.CreatorId != currentUserId)
                {
                    var assignment = await _assignRepo.GetAssignmentAsync(req.TaskId, currentUserId);
                    if (assignment is null)
                    {
                        throw ApiException.Forbidden("You do not have permission to edit this task.");
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(req.Title))
            {
                throw ApiException.BadRequest("Task title cannot be empty.");
            }

            task.Title = req.Title.Trim();
            task.Description = req.Description?.Trim();
            task.Deadline = NormalizeToUtc(req.Deadline);
            task.Status = ParseTaskStatus(req.Status, task.Status);
            await _taskRepo.SaveAsync();

            if (task.ProjectId.HasValue)
            {
                try
                {
                    var taskWithDetails = await _taskRepo.GetByIdWithDetailsAsync(task.Id);
                    if (taskWithDetails != null)
                    {
                        await _notificationService.SendTaskUpdatedAsync(task.ProjectId.Value, MapToTaskDetailResponse(taskWithDetails));
                    }
                }
                catch (Exception) { }
            }

            return "Task updated successfully.";
        }

        public async Task<string> DeleteTaskAsync(int taskId, Guid currentUserId)
        {
            var task = await GetTaskOrThrowAsync(taskId);

            if (task.ProjectId.HasValue)
            {
                await VerifyOwnerOrManagerAsync(task.ProjectId.Value, currentUserId, "You do not have permission to delete tasks in this project.");
            }
            else
            {
                if (task.CreatorId != currentUserId)
                {
                    throw ApiException.Forbidden("Only the task creator can delete this task.");
                }
            }

            var projectId = task.ProjectId;
            _taskRepo.Delete(task);
            await _taskRepo.SaveAsync();

            if (projectId.HasValue)
            {
                try
                {
                    await _notificationService.SendTaskDeletedAsync(projectId.Value, taskId);
                }
                catch (Exception) { }
            }

            return "Task deleted successfully.";
        }

        public async Task<string> AssignTaskAsync(AssignTaskRequest req, Guid currentUserId)
        {
            var task = await GetTaskOrThrowAsync(req.TaskId);

            if (task.ProjectId.HasValue)
            {
                await VerifyOwnerOrManagerAsync(task.ProjectId.Value, currentUserId, "You do not have permission to assign tasks in this project.");

                var targetMember = await _projectRepo.GetMemberAsync(task.ProjectId.Value, req.UserId);
                if (targetMember is null)
                {
                    throw ApiException.BadRequest("The assignee must be a project member.");
                }
            }
            else
            {
                if (task.CreatorId != currentUserId)
                {
                    throw ApiException.Forbidden("Only the task creator can assign tasks.");
                }
            }

            if (req.UserId == currentUserId)
            {
                throw ApiException.BadRequest("Cannot assign a task to yourself.");
            }

            if (await _assignRepo.ExistsAsync(req.TaskId, req.UserId))
            {
                throw ApiException.Conflict("This user has already been assigned to this task.");
            }

            _assignRepo.Add(new TaskAssignment
            {
                TaskId = req.TaskId,
                UserId = req.UserId,
                AssignedAt = DateTime.UtcNow
            });
            await _assignRepo.SaveAsync();

            try
            {
                var projectName = "";
                if (task.ProjectId.HasValue)
                {
                    var project = await _projectRepo.GetByIdAsync(task.ProjectId.Value);
                    if (project != null)
                    {
                        projectName = $" in project '{project.Name}'";
                    }
                }
                await _notificationService.CreateAndSendNotificationAsync(
                    req.UserId,
                    "New Task Assigned",
                    $"You have been assigned the task '{task.Title}'{projectName}.",
                    "TaskAssigned",
                    task.Id.ToString()
                );
            }
            catch (Exception)
            {
                // Soft fail
            }

            if (task.ProjectId.HasValue)
            {
                try
                {
                    var taskWithDetails = await _taskRepo.GetByIdWithDetailsAsync(task.Id);
                    if (taskWithDetails != null)
                    {
                        await _notificationService.SendTaskUpdatedAsync(task.ProjectId.Value, MapToTaskDetailResponse(taskWithDetails));
                    }
                }
                catch (Exception) { }
            }

            return "Task assigned successfully.";
        }

        public async Task<List<TaskDetailResponse>> GetProjectTasksAsync(Guid projectId, Guid userId)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, userId);
            if (member is null)
            {
                throw ApiException.Forbidden("You are not a member of this project.");
            }

            var tasks = await _taskRepo.GetTasksByProjectIdAsync(projectId);
            return tasks
                .Select(MapToTaskDetailResponse)
                .ToList();
        }


        public async Task<string> RemoveAssignmentAsync(RemoveAssignmentRequest req, Guid currentUserId)
        {
            var task = await GetTaskOrThrowAsync(req.TaskId);

            if (task.ProjectId.HasValue)
            {
                await VerifyOwnerOrManagerAsync(task.ProjectId.Value, currentUserId, "You do not have permission to revoke assignments in this project.");
            }
            else
            {
                if (task.CreatorId != currentUserId)
                {
                    throw ApiException.Forbidden("Only the task creator can revoke assignments.");
                }
            }

            var assignment = await _assignRepo.GetAssignmentAsync(req.TaskId, req.UserId);
            if (assignment is null)
            {
                throw ApiException.NotFound("This user has not been assigned to this task.");
            }

            _assignRepo.Remove(assignment);
            await _assignRepo.SaveAsync();

            if (task.ProjectId.HasValue)
            {
                try
                {
                    var taskWithDetails = await _taskRepo.GetByIdWithDetailsAsync(task.Id);
                    if (taskWithDetails != null)
                    {
                        await _notificationService.SendTaskUpdatedAsync(task.ProjectId.Value, MapToTaskDetailResponse(taskWithDetails));
                    }
                }
                catch (Exception) { }
            }

            return "Assignment revoked successfully.";
        }

        public async Task ChangeStatusAsync(ChangeTaskStatusRequest req, Guid currentUserId)
        {
            var task = await GetTaskOrThrowAsync(req.TaskId);

            if (task.ProjectId.HasValue)
            {
                var member = await GetMemberOrThrowAsync(task.ProjectId.Value, currentUserId);

                if (!IsOwnerOrManager(member))
                {
                    var isAssigned = await _assignRepo.ExistsAsync(req.TaskId, currentUserId);
                    if (!isAssigned)
                    {
                        throw ApiException.Forbidden("Members can only update status of tasks assigned to themselves.");
                    }
                }
            }
            else
            {
                if (task.CreatorId != currentUserId)
                {
                    var isAssigned = await _assignRepo.ExistsAsync(req.TaskId, currentUserId);
                    if (!isAssigned)
                    {
                        throw ApiException.Forbidden("You do not have permission to change the status of this task.");
                    }
                }
            }

            task.Status = ParseTaskStatus(req.Status, task.Status);
            await _taskRepo.SaveAsync();

            if (task.ProjectId.HasValue)
            {
                try
                {
                    var taskWithDetails = await _taskRepo.GetByIdWithDetailsAsync(task.Id);
                    if (taskWithDetails != null)
                    {
                        await _notificationService.SendTaskUpdatedAsync(task.ProjectId.Value, MapToTaskDetailResponse(taskWithDetails));
                    }
                }
                catch (Exception) { }
            }
        }

        private async Task<TodoTask> GetTaskOrThrowAsync(int taskId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task is null)
            {
                throw ApiException.NotFound($"Task #{taskId} does not exist.");
            }
            return task;
        }

        private static DateTime? NormalizeToUtc(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.Value.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc),
                DateTimeKind.Local => value.Value.ToUniversalTime(),
                _ => value.Value
            };
        }

        private TaskStatusModel ParseTaskStatus(string? status, TaskStatusModel defaultValue)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return defaultValue;
            }

            if (!Enum.TryParse(status, true, out TaskStatusModel parsed))
            {
                var validValues = string.Join(", ", Enum.GetNames(typeof(TaskStatusModel)));
                throw ApiException.BadRequest($"Status '{status}' is invalid. Valid values: {validValues}.");
            }

            return parsed;
        }

        private TaskDetailResponse MapToTaskDetailResponse(TodoTask task)
        {
            return new TaskDetailResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                CreatorId = task.CreatorId,
                Deadline = task.Deadline,
                Status = task.Status.ToString(),
                AssignedUsers = task.Assignments?
                    .Select(a => new AssignedUserResponse
                    {
                        UserId = a.UserId,
                        Email = a.User?.Email
                    })
                    .ToList()
            };
        }

        private async Task VerifyOwnerOrManagerAsync(Guid projectId, Guid userId, string errorMessage)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, userId);
            if (member is null)
            {
                throw ApiException.Forbidden("You are not a member of this project.");
            }
            if (!IsOwnerOrManager(member))
            {
                throw ApiException.Forbidden(errorMessage);
            }
        }

        private async Task<ProjectMember> GetMemberOrThrowAsync(Guid projectId, Guid userId, string errorMessage = "You do not have access to this project.")
        {
            var member = await _projectRepo.GetMemberAsync(projectId, userId);
            if (member is null)
            {
                throw ApiException.Forbidden(errorMessage);
            }
            return member;
        }

        private bool IsOwnerOrManager(ProjectMember member)
        {
            return member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) ||
                   member.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase);
        }
    }
}
