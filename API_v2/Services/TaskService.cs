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

        public TaskService(ITaskRepository taskRepo, ITaskAssignmentRepository assignRepo, IProjectRepository projectRepo)
        {
            _taskRepo = taskRepo;
            _assignRepo = assignRepo;
            _projectRepo = projectRepo;
        }

        public string CreateTask(CreateTaskRequest req, Guid creatorId, Guid projectId)
        {
            var member = _projectRepo.GetMember(projectId, creatorId);
            if (member is null)
            {
                throw ApiException.Forbidden("You are not a member of this project.");
            }
            if (!member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) &&
                !member.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
            {
                throw ApiException.Forbidden("Only Owners or Managers can create tasks.");
            }

            if (string.IsNullOrWhiteSpace(req.Title))
            {
                throw ApiException.BadRequest("Task title cannot be empty.");
            }

            var status = ParseTaskStatus(req.Status, TaskStatusModel.ToDo);
            _taskRepo.Add(new TodoTask
            {
                Title = req.Title.Trim(),
                Description = req.Description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatorId = creatorId,
                Deadline = NormalizeToUtc(req.Deadline),
                Status = status,
                ProjectId = projectId
            });
            _taskRepo.Save();

            return "Task created successfully.";
        }

        public string UpdateTask(UpdateTaskRequest req, Guid currentUserId)
        {
            var task = GetTaskOrThrow(req.TaskId);

            if (task.ProjectId.HasValue)
            {
                var member = _projectRepo.GetMember(task.ProjectId.Value, currentUserId);
                if (member is null ||
                    (!member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) &&
                     !member.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase)))
                {
                    throw ApiException.Forbidden("You do not have permission to edit tasks in this project.");
                }
            }
            else
            {
                if (task.CreatorId != currentUserId)
                {
                    var assignment = _assignRepo.GetAssignment(req.TaskId, currentUserId);
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
            _taskRepo.Save();

            return "Task updated successfully.";
        }

        public string DeleteTask(int taskId, Guid currentUserId)
        {
            var task = GetTaskOrThrow(taskId);

            if (task.ProjectId.HasValue)
            {
                var member = _projectRepo.GetMember(task.ProjectId.Value, currentUserId);
                if (member is null ||
                    (!member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) &&
                     !member.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase)))
                {
                    throw ApiException.Forbidden("You do not have permission to delete tasks in this project.");
                }
            }
            else
            {
                if (task.CreatorId != currentUserId)
                {
                    throw ApiException.Forbidden("Only the task creator can delete this task.");
                }
            }

            _taskRepo.Delete(task);
            _taskRepo.Save();
            return "Task deleted successfully.";
        }

        public string AssignTask(AssignTaskRequest req, Guid currentUserId)
        {
            var task = GetTaskOrThrow(req.TaskId);

            if (task.ProjectId.HasValue)
            {
                var member = _projectRepo.GetMember(task.ProjectId.Value, currentUserId);
                if (member is null ||
                    (!member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) &&
                     !member.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase)))
                {
                    throw ApiException.Forbidden("You do not have permission to assign tasks in this project.");
                }

                var targetMember = _projectRepo.GetMember(task.ProjectId.Value, req.UserId);
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

            if (_assignRepo.Exists(req.TaskId, req.UserId))
            {
                throw ApiException.Conflict("This user has already been assigned to this task.");
            }

            _assignRepo.Add(new TaskAssignment
            {
                TaskId = req.TaskId,
                UserId = req.UserId,
                AssignedAt = DateTime.UtcNow
            });
            _assignRepo.Save();

            return "Task assigned successfully.";
        }

        public List<TaskDetailResponse> GetProjectTasks(Guid projectId, Guid userId)
        {
            var member = _projectRepo.GetMember(projectId, userId);
            if (member is null)
            {
                throw ApiException.Forbidden("You are not a member of this project.");
            }

            return _taskRepo.GetTasksByProjectId(projectId)
                .Select(MapToTaskDetailResponse)
                .ToList();
        }


        public string RemoveAssignment(RemoveAssignmentRequest req, Guid currentUserId)
        {
            var task = GetTaskOrThrow(req.TaskId);

            if (task.ProjectId.HasValue)
            {
                var member = _projectRepo.GetMember(task.ProjectId.Value, currentUserId);
                if (member is null ||
                    (!member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) &&
                     !member.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase)))
                {
                    throw ApiException.Forbidden("You do not have permission to revoke assignments in this project.");
                }
            }
            else
            {
                if (task.CreatorId != currentUserId)
                {
                    throw ApiException.Forbidden("Only the task creator can revoke assignments.");
                }
            }

            var assignment = _assignRepo.GetAssignment(req.TaskId, req.UserId);
            if (assignment is null)
            {
                throw ApiException.NotFound("This user has not been assigned to this task.");
            }

            _assignRepo.Remove(assignment);
            _assignRepo.Save();
            return "Assignment revoked successfully.";
        }

        public void ChangeStatus(ChangeTaskStatusRequest req, Guid currentUserId)
        {
            var task = GetTaskOrThrow(req.TaskId);

            if (task.ProjectId.HasValue)
            {
                var member = _projectRepo.GetMember(task.ProjectId.Value, currentUserId);
                if (member is null)
                {
                    throw ApiException.Forbidden("You do not have access to this project.");
                }

                if (!member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) &&
                    !member.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                {
                    var isAssigned = _assignRepo.Exists(req.TaskId, currentUserId);
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
                    var isAssigned = _assignRepo.Exists(req.TaskId, currentUserId);
                    if (!isAssigned)
                    {
                        throw ApiException.Forbidden("You do not have permission to change the status of this task.");
                    }
                }
            }

            task.Status = ParseTaskStatus(req.Status, task.Status);
            _taskRepo.Save();
        }

        private TodoTask GetTaskOrThrow(int taskId)
        {
            var task = _taskRepo.GetById(taskId);
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
    }
}
