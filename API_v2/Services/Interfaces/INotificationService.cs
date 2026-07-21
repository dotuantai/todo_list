using System;
using System.Collections.Generic;
using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface INotificationService
    {
        List<NotificationResponse> GetNotifications(Guid userId);
        void MarkAsRead(Guid notificationId, Guid userId);
        void MarkAllAsRead(Guid userId);
        void CreateAndSendNotification(Guid userId, string title, string message, string type, string referenceId);
    }
}
