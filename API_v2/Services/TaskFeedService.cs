using System.Text.Json;
using API_v2.Datas;
using API_v2.Exceptions;
using API_v2.Models.DTOs;
using API_v2.Models.Constants;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Services
{
    public class TaskFeedService : ITaskFeedService
    {
        private readonly AppDbContext _db;
        private readonly IProjectRepository _projectRepo;

        public TaskFeedService(AppDbContext db, IProjectRepository projectRepo)
        {
            _db = db;
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

            var task = await _db.Tasks
                .AsNoTracking()
                .Where(item => item.Id == taskId)
                .Select(item => new { item.ProjectId, item.CreatorId })
                .FirstOrDefaultAsync();
            if (task == null)
            {
                throw ApiException.NotFound($"Task #{taskId} not found.", ErrorCodes.TaskNotFound);
            }

            if (task.ProjectId.HasValue)
            {
                var member = await _projectRepo.GetMemberAsync(task.ProjectId.Value, currentUserId);
                if (member == null)
                {
                    throw ApiException.Forbidden("You do not have access to this project.");
                }
            }
            else if (task.CreatorId != currentUserId &&
                     !await _db.TaskAssignments.AnyAsync(item => item.TaskId == taskId && item.UserId == currentUserId))
            {
                throw ApiException.Forbidden("You do not have access to this task.");
            }

            var comments = _db.TaskComments
                .AsNoTracking()
                .Where(comment => comment.TaskId == taskId)
                .Select(comment => new TaskFeedDatabaseRow
                {
                    Type = "comment",
                    Id = comment.Id,
                    CreatedAt = comment.CreatedAt,
                    UserId = comment.UserId,
                    UserName = comment.User.Email,
                    Content = comment.Content,
                    ChangesJson = null
                });

            var activities = _db.TaskActivities
                .AsNoTracking()
                .Where(activity => activity.TaskId == taskId)
                .Select(activity => new TaskFeedDatabaseRow
                {
                    Type = "activity",
                    Id = activity.Id,
                    CreatedAt = activity.ChangedAt,
                    UserId = activity.UserId,
                    UserName = activity.User.Email,
                    Content = null,
                    ChangesJson = activity.Changes
                });

            var combined = comments.Concat(activities);
            var totalCount = await combined.CountAsync();
            var rows = await combined
                .OrderByDescending(item => item.CreatedAt)
                .ThenByDescending(item => item.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = rows.Select(MapRow).Reverse().ToList();
            return new PagedResponse<TaskFeedItemDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private static TaskFeedItemDto MapRow(TaskFeedDatabaseRow row)
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

        private sealed class TaskFeedDatabaseRow
        {
            public string Type { get; init; } = string.Empty;
            public int Id { get; init; }
            public DateTime CreatedAt { get; init; }
            public Guid UserId { get; init; }
            public string UserName { get; init; } = string.Empty;
            public string? Content { get; init; }
            public string? ChangesJson { get; init; }
        }
    }
}
