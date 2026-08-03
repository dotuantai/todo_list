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
        private readonly IProjectColumnRepository _projectColumnRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<TaskService> _logger;
        private readonly API_v2.Datas.AppDbContext _db;

        public TaskService(
            ITaskRepository taskRepo, 
            ITaskAssignmentRepository assignRepo, 
            IProjectRepository projectRepo,
            IProjectColumnRepository projectColumnRepo,
            INotificationService notificationService,
            API_v2.Datas.AppDbContext db,
            ILogger<TaskService> logger)
        {
            _taskRepo = taskRepo;
            _assignRepo = assignRepo;
            _projectRepo = projectRepo;
            _projectColumnRepo = projectColumnRepo;
            _notificationService = notificationService;
            _db = db;
            _logger = logger;
        }

        public async Task<string> CreateTaskAsync(CreateTaskRequest req, Guid creatorId, Guid projectId)
        {
            await VerifyOwnerOrManagerAsync(projectId, creatorId, "Only Owners or Managers can create tasks.");

            if (string.IsNullOrWhiteSpace(req.Title))
            {
                throw ApiException.BadRequest("Task title cannot be empty.");
            }

            await VerifyColumnBelongsToProjectAsync(req.ColumnId, projectId);

            var task = new TodoTask
            {
                Title = req.Title.Trim(),
                Description = req.Description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatorId = creatorId,
                Deadline = NormalizeToUtc(req.Deadline),
                ColumnId = req.ColumnId,
                ProjectId = projectId,
                Priority = req.Priority
            };
            _taskRepo.Add(task);
            await _taskRepo.SaveAsync();

            if (!string.IsNullOrWhiteSpace(req.AssigneeId) && Guid.TryParse(req.AssigneeId, out Guid parsedUserId))
            {
                var assignment = new TaskAssignment
                {
                    TaskId = task.Id,
                    UserId = parsedUserId,
                    AssignedAt = DateTime.UtcNow
                };
                _assignRepo.Add(assignment);
                await _assignRepo.SaveAsync();
            }

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
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var task = await _taskRepo.GetByIdWithDetailsAsync(taskId);
                if (task is null)
                {
                    throw ApiException.NotFound($"Task #{taskId} does not exist.");
                }

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

                await VerifyColumnBelongsToProjectAsync(req.ColumnId, task.ProjectId);

                task.Title = req.Title.Trim();
                task.Description = req.Description?.Trim();
                task.Deadline = NormalizeToUtc(req.Deadline);
                task.ColumnId = req.ColumnId;
                task.Priority = req.Priority;
                
                // Update assignments if provided
                if (req.AssignedUserIds != null)
                {
                    // Remove existing assignments not in the new list
                    var currentAssigneeIds = task.Assignments.Select(a => a.UserId).ToList();
                    
                    var toRemove = task.Assignments.Where(a => !req.AssignedUserIds.Contains(a.UserId.ToString())).ToList();
                    foreach (var a in toRemove)
                    {
                        _assignRepo.Remove(a);
                    }

                    // Add new assignments
                    foreach (var userIdStr in req.AssignedUserIds)
                    {
                        if (Guid.TryParse(userIdStr, out Guid parsedUserId) && !currentAssigneeIds.Contains(parsedUserId))
                        {
                            _assignRepo.Add(new TaskAssignment
                            {
                                TaskId = task.Id,
                                UserId = parsedUserId,
                                AssignedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                await _taskRepo.SaveAsync(); // this saves both task and assignments due to context tracking
                await transaction.CommitAsync();

                if (task.ProjectId.HasValue)
                {
                    try
                    {
                        // Refresh details to include newly added assignments for notification
                        var updatedTaskWithDetails = await _taskRepo.GetByIdWithDetailsAsync(task.Id);
                        if (updatedTaskWithDetails != null)
                        {
                            await _notificationService.SendTaskUpdatedAsync(task.ProjectId.Value, MapToTaskDetailResponse(updatedTaskWithDetails));
                        }
                    }
                    catch (Exception ex) { _logger.LogWarning(ex, "Failed to send task updated notification."); }
                }

                return "Task updated successfully.";
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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

        public async Task<PagedResponse<TaskDetailResponse>> GetProjectTasksAsync(Guid projectId, Guid userId, int? columnId, int page, int pageSize, string search = null, API_v2.Models.Enums.TaskPriority? priority = null, Guid? assigneeId = null)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, userId);
            if (member is null)
            {
                throw ApiException.Forbidden("You are not a member of this project.");
            }

            var (items, totalCount) = await _taskRepo.GetTasksByProjectIdAsync(projectId, columnId, page, pageSize, search, priority, assigneeId);
            
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

            await VerifyColumnBelongsToProjectAsync(req.ColumnId, task.ProjectId);
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
                Priority = task.Priority,
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

        private async Task VerifyColumnBelongsToProjectAsync(int columnId, Guid? projectId)
        {
            if (!projectId.HasValue) return;
            
            var column = await _projectColumnRepo.GetByIdAsync(columnId);
            if (column == null || column.ProjectId != projectId.Value)
            {
                throw ApiException.BadRequest("The specified column does not belong to this project.");
            }
        }
    }
}
