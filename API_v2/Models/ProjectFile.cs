using System;
using System.Collections.Generic;

namespace API_v2.Models
{
    public class ProjectFile
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? FolderId { get; set; }
        public int? TaskId { get; set; }
        public string GoogleDriveFileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? MimeType { get; set; }
        public int CurrentVersion { get; set; } = 1;
        public Guid UploadedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedById { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ProjectFolder? Folder { get; set; }
        public virtual TodoTask? Task { get; set; }
        public virtual User UploadedBy { get; set; } = null!;
        public virtual User? UpdatedBy { get; set; }
        public virtual ICollection<ProjectFileVersion> Versions { get; set; } = new List<ProjectFileVersion>();
    }
}
