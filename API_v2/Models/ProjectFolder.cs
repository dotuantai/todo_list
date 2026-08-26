using System;
using System.Collections.Generic;

namespace API_v2.Models
{
    public class ProjectFolder
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? ParentFolderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? GoogleDriveFolderId { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ProjectFolder? ParentFolder { get; set; }
        public virtual ICollection<ProjectFolder> SubFolders { get; set; } = new List<ProjectFolder>();
        public virtual ICollection<ProjectFile> Files { get; set; } = new List<ProjectFile>();
        public virtual User CreatedBy { get; set; } = null!;
    }
}
