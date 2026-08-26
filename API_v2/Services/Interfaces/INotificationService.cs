using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface INotificationService
    {
        Task<PagedResponse<NotificationResponse>> GetNotificationsAsync(Guid userId, int page, int pageSize);
        Task MarkAsReadAsync(Guid notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
        Task CreateAndSendNotificationAsync(Guid userId, string title, string message, string type, string referenceId);
        Task SendTaskCreatedAsync(Guid projectId, TaskDetailResponse task);
        Task SendTaskUpdatedAsync(Guid projectId, TaskDetailResponse task);
        Task SendTaskDeletedAsync(Guid projectId, int taskId);
        Task SendColumnsReorderedAsync(Guid projectId, IReadOnlyList<ProjectColumnResponse> columns);
    }
}
