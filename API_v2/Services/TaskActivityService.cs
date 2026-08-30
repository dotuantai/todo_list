using System.Text.Json;
using API_v2.Exceptions;
using API_v2.Models.DTOs;
using API_v2.Models.Constants;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class TaskActivityService : ITaskActivityService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskActivityRepository _activityRepository;
        private readonly IProjectRepository _projectRepository;

        public TaskActivityService(ITaskRepository taskRepository, ITaskActivityRepository activityRepository, IProjectRepository projectRepository)
        {
            _taskRepository = taskRepository;
            _activityRepository = activityRepository;
            _projectRepository = projectRepository;
        }

        public async Task<List<TaskActivityResponse>> GetActivitiesAsync(int taskId, Guid currentUserId)
        {
            var task = await _taskRepository.GetByIdWithDetailsAsync(taskId)
                ?? throw ApiException.NotFound($"Task #{taskId} not found.", ErrorCodes.TaskNotFound);

            if (task.ProjectId.HasValue)
            {
                var member = await _projectRepository.GetMemberAsync(task.ProjectId.Value, currentUserId);
                if (member is null && !await _projectRepository.IsSystemAdminAsync(currentUserId))
                {
                    throw ApiException.Forbidden("You do not have access to this task.");
                }
            }
            else if (task.CreatorId != currentUserId && !task.Assignments.Any(assignment => assignment.UserId == currentUserId))
            {
                throw ApiException.Forbidden("You do not have access to this task.");
            }

            var activities = await _activityRepository.GetByTaskIdAsync(taskId);

            return activities.Select(activity => new TaskActivityResponse
            {
                Id = activity.Id,
                TaskId = activity.TaskId,
                UserId = activity.UserId,
                UserEmail = activity.User.Email,
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
