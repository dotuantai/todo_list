using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using API_v2.Exceptions;
using API_v2.Hubs;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositorys.IRepositorys;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(INotificationRepository notificationRepo, IHubContext<NotificationHub> hubContext)
        {
            _notificationRepo = notificationRepo;
            _hubContext = hubContext;
        }

        public async Task<List<NotificationResponse>> GetNotificationsAsync(Guid userId)
        {
            var notifications = await _notificationRepo.GetNotificationsByUserIdAsync(userId);
            return notifications.Select(n => new NotificationResponse
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                Type = n.Type,
                ReferenceId = n.ReferenceId
            }).ToList();
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notif = await _notificationRepo.GetNotificationByIdAndUserIdAsync(notificationId, userId);
            if (notif == null)
            {
                throw ApiException.NotFound("Notification not found.");
            }

            notif.IsRead = true;
            await _notificationRepo.SaveAsync();
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unread = await _notificationRepo.GetUnreadNotificationsByUserIdAsync(userId);
            foreach (var n in unread)
            {
                n.IsRead = true;
            }
            await _notificationRepo.SaveAsync();
        }

        public async Task CreateAndSendNotificationAsync(Guid userId, string title, string message, string type, string referenceId)
        {
            var notif = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Type = type,
                ReferenceId = referenceId
            };

            _notificationRepo.Add(notif);
            await _notificationRepo.SaveAsync();

            var resp = new NotificationResponse
            {
                Id = notif.Id,
                Title = notif.Title,
                Message = notif.Message,
                IsRead = notif.IsRead,
                CreatedAt = notif.CreatedAt,
                Type = notif.Type,
                ReferenceId = notif.ReferenceId
            };

            // Send in real-time via SignalR
            await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", resp);
        }

        public async Task SendTaskCreatedAsync(Guid projectId, TaskDetailResponse task)
        {
            await _hubContext.Clients.Group($"Project_{projectId}").SendAsync("TaskCreated", task);
        }

        public async Task SendTaskUpdatedAsync(Guid projectId, TaskDetailResponse task)
        {
            await _hubContext.Clients.Group($"Project_{projectId}").SendAsync("TaskUpdated", task);
        }

        public async Task SendTaskDeletedAsync(Guid projectId, int taskId)
        {
            await _hubContext.Clients.Group($"Project_{projectId}").SendAsync("TaskDeleted", taskId);
        }
    }
}
