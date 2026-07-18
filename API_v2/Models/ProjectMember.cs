using System;

namespace API_v2.Models
{
    public class ProjectMember
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
