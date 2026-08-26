using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _db;

        public NotificationRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(List<Notification> Items, int TotalCount)> GetNotificationsByUserIdAsync(Guid userId, int page, int pageSize)
        {
            var query = _db.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Notification?> GetNotificationByIdAndUserIdAsync(Guid notificationId, Guid userId)
        {
            return await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        }

        public async Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(Guid userId)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
        }

        public void Add(Notification notification)
        {
            _db.Notifications.Add(notification);
        }

        public async Task DeleteOldNotificationsAsync(DateTime cutoff)
        {
            await _db.Notifications
                .Where(n => n.CreatedAt < cutoff)
                .ExecuteDeleteAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
