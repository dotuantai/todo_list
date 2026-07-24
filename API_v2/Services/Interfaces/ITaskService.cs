using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface ITaskService
    {
        Task<string> CreateTaskAsync(CreateTaskRequest req, Guid creatorId, Guid projectId);
        Task<string> UpdateTaskAsync(UpdateTaskRequest req, Guid currentUserId);
        Task<string> DeleteTaskAsync(int taskId, Guid currentUserId);
        Task<string> AssignTaskAsync(AssignTaskRequest req, Guid currentUserId);
        Task<List<TaskDetailResponse>> GetProjectTasksAsync(Guid projectId, Guid userId);
        Task<string> RemoveAssignmentAsync(RemoveAssignmentRequest req, Guid currentUserId);
        Task ChangeStatusAsync(ChangeTaskStatusRequest req, Guid currentUserId);
    }
}
