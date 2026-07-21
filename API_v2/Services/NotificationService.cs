using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using API_v2.Datas;
using API_v2.Exceptions;
using API_v2.Hubs;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(AppDbContext db, IHubContext<NotificationHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        public List<NotificationResponse> GetNotifications(Guid userId)
        {
            return _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponse
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    Type = n.Type,
                    ReferenceId = n.ReferenceId
                })
                .ToList();
        }

        public void MarkAsRead(Guid notificationId, Guid userId)
        {
            var notif = _db.Notifications.FirstOrDefault(n => n.Id == notificationId && n.UserId == userId);
            if (notif == null)
            {
                throw ApiException.NotFound("Notification not found.");
            }

            notif.IsRead = true;
            _db.SaveChanges();
        }

        public void MarkAllAsRead(Guid userId)
        {
            var unread = _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToList();
            foreach (var n in unread)
            {
                n.IsRead = true;
            }
            _db.SaveChanges();
        }

        public void CreateAndSendNotification(Guid userId, string title, string message, string type, string referenceId)
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

            _db.Notifications.Add(notif);
            _db.SaveChanges();

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
            _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", resp);
        }
    }
}
