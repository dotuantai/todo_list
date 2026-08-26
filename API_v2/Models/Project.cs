using System;
using System.Collections.Generic;

namespace API_v2.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid OwnerId { get; set; }
        public string? GoogleDriveFolderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual User Owner { get; set; } = null!;
        public virtual ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
        public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
        public virtual ICollection<ProjectColumn> Columns { get; set; } = new List<ProjectColumn>();
        public virtual ICollection<ProjectFile> Files { get; set; } = new List<ProjectFile>();
        public virtual ICollection<ProjectFolder> Folders { get; set; } = new List<ProjectFolder>();
        public virtual ICollection<ProjectFileActivity> FileActivities { get; set; } = new List<ProjectFileActivity>();
    }
}
