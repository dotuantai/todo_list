using System;

namespace API_v2.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Type { get; set; }
        public string? ReferenceId { get; set; }

        // Navigation
        public User? User { get; set; }
    }
}
