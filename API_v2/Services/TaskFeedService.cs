using System.Text.Json;
using API_v2.Exceptions;
using API_v2.Models.DTOs;
using API_v2.Models.Constants;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class TaskFeedService : ITaskFeedService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskAssignmentRepository _assignmentRepository;
        private readonly ITaskFeedRepository _feedRepository;
        private readonly IProjectRepository _projectRepo;

        public TaskFeedService(ITaskRepository taskRepository, ITaskAssignmentRepository assignmentRepository, ITaskFeedRepository feedRepository, IProjectRepository projectRepo)
        {
            _taskRepository = taskRepository;
            _assignmentRepository = assignmentRepository;
            _feedRepository = feedRepository;
            _projectRepo = projectRepo;
        }

        public async Task<PagedResponse<TaskFeedItemDto>> GetTaskFeedAsync(
            int taskId,
            Guid currentUserId,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw ApiException.NotFound($"Task #{taskId} not found.", ErrorCodes.TaskNotFound);
            }

            if (task.ProjectId.HasValue)
            {
                var member = await _projectRepo.GetMemberAsync(task.ProjectId.Value, currentUserId);
                if (member == null && !await _projectRepo.IsSystemAdminAsync(currentUserId))
                {
                    throw ApiException.Forbidden("You do not have access to this project.");
                }
            }
            else if (task.CreatorId != currentUserId &&
                     !await _assignmentRepository.ExistsAsync(taskId, currentUserId))
            {
                throw ApiException.Forbidden("You do not have access to this task.");
            }

            var (rows, totalCount) = await _feedRepository.GetTaskFeedAsync(taskId, page, pageSize);

            var items = rows.Select(MapRow).Reverse().ToList();
            return new PagedResponse<TaskFeedItemDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private static TaskFeedItemDto MapRow(TaskFeedRecord row)
        {
            List<FieldChangeDto>? changes = null;
            if (!string.IsNullOrWhiteSpace(row.ChangesJson))
            {
                try
                {
                    changes = JsonSerializer.Deserialize<List<FieldChangeDto>>(row.ChangesJson);
                }
                catch (JsonException)
                {
                    changes = [];
                }
            }

            return new TaskFeedItemDto
            {
                Type = row.Type,
                Id = row.Id,
                CreatedAt = row.CreatedAt,
                UserId = row.UserId,
                UserName = row.UserName,
                Content = row.Content,
                Changes = changes
            };
        }

    }
}
