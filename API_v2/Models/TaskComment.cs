using System;
using System.ComponentModel.DataAnnotations;

namespace API_v2.Models
{
    public class TaskComment
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual TodoTask Task { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
