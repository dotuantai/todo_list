using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface ITaskActivityService
    {
        Task<List<TaskActivityResponse>> GetActivitiesAsync(int taskId, Guid currentUserId);
    }
}
