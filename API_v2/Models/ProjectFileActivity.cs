using System;

namespace API_v2.Models
{
    public class ProjectFileActivity
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string ActionType { get; set; } = string.Empty; // CreateFolder, UploadFile, UpdateVersion, RenameFile, DeleteFile, DeleteFolder
        public string TargetName { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
