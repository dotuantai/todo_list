using System;

namespace API_v2.Models
{
    public class ProjectFileVersion
    {
        public Guid Id { get; set; }
        public Guid ProjectFileId { get; set; }
        public int VersionNumber { get; set; }
        public string GoogleDriveFileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? MimeType { get; set; }
        public string? ChangeNote { get; set; }
        public Guid UploadedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ProjectFile ProjectFile { get; set; } = null!;
        public virtual User UploadedBy { get; set; } = null!;
    }
}
