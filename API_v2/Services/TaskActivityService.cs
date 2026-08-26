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
    public class TaskActivityService : ITaskActivityService
    {
        private readonly AppDbContext _db;
        private readonly IProjectRepository _projectRepository;

        public TaskActivityService(AppDbContext db, IProjectRepository projectRepository)
        {
            _db = db;
            _projectRepository = projectRepository;
        }

        public async Task<List<TaskActivityResponse>> GetActivitiesAsync(int taskId, Guid currentUserId)
        {
            var task = await _db.Tasks
                .AsNoTracking()
                .Where(t => t.Id == taskId)
                .Select(t => new
                {
                    t.ProjectId,
                    t.CreatorId,
                    IsAssigned = t.Assignments.Any(a => a.UserId == currentUserId)
                })
                .FirstOrDefaultAsync()
                ?? throw ApiException.NotFound($"Task #{taskId} not found.", ErrorCodes.TaskNotFound);

            if (task.ProjectId.HasValue)
            {
                var member = await _projectRepository.GetMemberAsync(task.ProjectId.Value, currentUserId);
                if (member is null)
                {
                    throw ApiException.Forbidden("You do not have access to this task.");
                }
            }
            else if (task.CreatorId != currentUserId && !task.IsAssigned)
            {
                throw ApiException.Forbidden("You do not have access to this task.");
            }

            var activities = await _db.TaskActivities
                .AsNoTracking()
                .Where(activity => activity.TaskId == taskId)
                .OrderBy(activity => activity.ChangedAt)
                .Select(activity => new
                {
                    activity.Id,
                    activity.TaskId,
                    activity.UserId,
                    UserEmail = activity.User.Email,
                    activity.ChangedAt,
                    activity.Changes
                })
                .ToListAsync();

            return activities.Select(activity => new TaskActivityResponse
            {
                Id = activity.Id,
                TaskId = activity.TaskId,
                UserId = activity.UserId,
                UserEmail = activity.UserEmail,
                ChangedAt = activity.ChangedAt,
                Changes = DeserializeChanges(activity.Changes)
            }).ToList();
        }

        private static List<FieldChangeDto> DeserializeChanges(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<List<FieldChangeDto>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }
    }
}
