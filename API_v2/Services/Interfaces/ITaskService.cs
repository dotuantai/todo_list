using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface ITaskService
    {
        Task<string> CreateTaskAsync(CreateTaskRequest req, Guid creatorId, Guid projectId);
        Task<string> UpdateTaskAsync(int taskId, UpdateTaskRequest req, Guid currentUserId);
        Task<string> DeleteTaskAsync(int taskId, Guid currentUserId);
        Task<string> AssignTaskAsync(AssignTaskRequest req, Guid currentUserId);
        Task<PagedResponse<TaskDetailResponse>> GetProjectTasksAsync(Guid projectId, Guid userId, int? columnId, int page, int pageSize, string search = null, API_v2.Models.Enums.TaskPriority? priority = null, Guid? assigneeId = null);
        Task<TaskStatsResponse> GetTaskStatsAsync(Guid projectId, Guid userId);
        Task<string> RemoveAssignmentAsync(int taskId, Guid userId, Guid currentUserId);
        Task ChangeColumnAsync(ChangeTaskColumnRequest req, Guid currentUserId);
    }
}
