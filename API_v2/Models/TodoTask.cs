using System;
using System.Collections.Generic;
using API_v2.Models.Enums;

namespace API_v2.Models
{
    public class TodoTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? StartDate { get; set; }
        public double? EstimatedHours { get; set; }
        public double? ActualHours { get; set; }
        public int ColumnId { get; set; }
        public Guid CreatorId { get; set; }
        public Guid? ProjectId { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        // Navigation properties
        public virtual User Creator { get; set; } = null!;
        public virtual Project? Project { get; set; }
        public virtual ProjectColumn Column { get; set; } = null!;
        public virtual ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
        public virtual ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    }
}
