using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface ITaskService
    {
        string CreateTask(CreateTaskRequest req, Guid creatorId, Guid projectId);
        string UpdateTask(UpdateTaskRequest req, Guid currentUserId);
        string DeleteTask(int taskId, Guid currentUserId);
        string AssignTask(AssignTaskRequest req, Guid currentUserId);
        List<TaskDetailResponse> GetProjectTasks(Guid projectId, Guid userId);
        string UpdatePermission(AssignTaskRequest req, Guid currentUserId);
        string RemoveAssignment(RemoveAssignmentRequest req, Guid currentUserId);
        void ChangeStatus(ChangeTaskStatusRequest req, Guid currentUserId);
    }
}
