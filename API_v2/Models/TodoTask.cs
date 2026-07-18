using System;
using System.Collections.Generic;

namespace API_v2.Models
{
    public enum TaskStatus
    {
        ToDo = 0,
        InProgress = 1,
        Done = 2,
        Closed = 3
    }

    public class TodoTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public TaskStatus Status { get; set; }
        public Guid CreatorId { get; set; }
        public Guid? ProjectId { get; set; }

        // Navigation properties
        public virtual User Creator { get; set; } = null!;
        public virtual Project? Project { get; set; }
        public virtual ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
    }
}
