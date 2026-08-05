using System;
using System.Collections.Generic;

namespace API_v2.Models
{
    public class TaskActivity
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public Guid UserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// JSON-serialized list of FieldChange: [{"Field":"Status","OldValue":"To Do","NewValue":"In Progress"},...]
        /// Description changes use OldValue=null, NewValue="__description_changed__"
        /// </summary>
        public string Changes { get; set; } = "[]";

        // Navigation
        public virtual TodoTask Task { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }

    public class FieldChange
    {
        public string Field { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }
}
