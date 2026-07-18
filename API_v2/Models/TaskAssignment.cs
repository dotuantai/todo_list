using System;

namespace API_v2.Models
{
    public class TaskAssignment
    {
        public int TaskId { get; set; }
        public Guid UserId { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public DateTime AssignedAt { get; set; }

        // Navigation properties
        public virtual TodoTask Task { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
