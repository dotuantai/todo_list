using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Exceptions;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Models.Constants;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepo;
        private readonly ITaskAssignmentRepository _assignRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            ITaskRepository taskRepo, 
            ITaskAssignmentRepository assignRepo, 
            IProjectRepository projectRepo,
            INotificationService notificationService,
            ILogger<TaskService> logger)
        {
            _taskRepo = taskRepo;
            _assignRepo = assignRepo;
            _projectRepo = projectRepo;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<string> CreateTaskAsync(CreateTaskRequest req, Guid creatorId, Guid projectId)
        {
            await VerifyOwnerOrManagerAsync(projectId, creatorId, "Only Owners or Managers can create tasks.");

            if (string.IsNullOrWhiteSpace(req.Title))
            {
                throw ApiException.BadRequest("Task title cannot be empty.");
            }

            var task = new TodoTask
            {
                Title = req.Title.Trim(),
                Description = req.Description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatorId = creatorId,
                Deadline = NormalizeToUtc(req.Deadline),
                ColumnId = req.ColumnId,
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send task created notification.");
            }

            return "Task created successfully.";
        }

        public async Task<string> UpdateTaskAsync(int taskId, UpdateTaskRequest req, Guid currentUserId)
        {
            var task = await GetTaskOrThrowAsync(taskId);

            if (task.ProjectId.HasValue)
            {
                await VerifyOwnerOrManagerAsync(task.ProjectId.Value, currentUserId, "You do not have permission to edit tasks in this project.");
            }
            else
            {
                if (task.CreatorId != currentUserId)
                {
                    var assignment = await _assignRepo.GetAssignmentAsync(taskId, currentUserId);
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
            task.ColumnId = req.ColumnId;
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
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to send task updated notification."); }
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
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to send task deleted notification."); }
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send task assigned notification.");
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
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to send task updated notification."); }
            }

            return "Task assigned successfully.";
        }

        public async Task<PagedResponse<TaskDetailResponse>> GetProjectTasksAsync(Guid projectId, Guid userId, int? columnId, int page, int pageSize)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, userId);
            if (member is null)
            {
                throw ApiException.Forbidden("You are not a member of this project.");
            }

            var (items, totalCount) = await _taskRepo.GetTasksByProjectIdAsync(projectId, columnId, page, pageSize);
            
            return new PagedResponse<TaskDetailResponse>
            {
                Items = items.Select(MapToTaskDetailResponse).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<TaskStatsResponse> GetTaskStatsAsync(Guid projectId, Guid userId)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, userId);
            if (member is null)
            {
                throw ApiException.Forbidden("You are not a member of this project.");
            }

            return await _taskRepo.GetTaskStatsByProjectIdAsync(projectId);
        }


        public async Task<string> RemoveAssignmentAsync(int taskId, Guid userId, Guid currentUserId)
        {
            var task = await GetTaskOrThrowAsync(taskId);

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

            var assignment = await _assignRepo.GetAssignmentAsync(taskId, userId);
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
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to send task updated notification."); }
            }

            return "Assignment revoked successfully.";
        }

        public async Task ChangeColumnAsync(ChangeTaskColumnRequest req, Guid currentUserId)
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

            task.ColumnId = req.ColumnId;
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
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to send task updated notification."); }
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
                ColumnId = task.ColumnId,
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
            return ProjectRoles.IsOwnerOrManager(member.Role);
        }
    }
}
