using System;
using System.Collections.Generic;

namespace API_v2.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool RequiresPasswordChange { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; }

        // Navigation properties
        public virtual ICollection<TodoTask> CreatedTasks { get; set; } = new List<TodoTask>();
        public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
        public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
        public virtual ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    }
}
