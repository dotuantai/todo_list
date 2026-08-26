using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface ITaskFeedService
    {
        Task<PagedResponse<TaskFeedItemDto>> GetTaskFeedAsync(
            int taskId,
            Guid currentUserId,
            int page,
            int pageSize);
    }
}
