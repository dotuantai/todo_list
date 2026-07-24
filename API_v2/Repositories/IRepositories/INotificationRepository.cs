using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models;

namespace API_v2.Repositories.IRepositories
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetNotificationsByUserIdAsync(Guid userId);
        Task<Notification?> GetNotificationByIdAndUserIdAsync(Guid notificationId, Guid userId);
        Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(Guid userId);
        void Add(Notification notification);
        Task DeleteOldNotificationsAsync(DateTime cutoff);
        Task SaveAsync();
    }
}
