using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace API_v2.Models.DTOs
{
    public class ProjectFolderResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? ParentFolderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? GoogleDriveFolderId { get; set; }
        public Guid CreatedById { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int FileCount { get; set; }
        public int SubFolderCount { get; set; }
    }

    public class CreateProjectFolderRequest
    {
        public string Name { get; set; } = string.Empty;
        public Guid? ParentFolderId { get; set; }
    }

    public class RenameProjectFolderRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class ProjectFileResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? FolderId { get; set; }
        public string? FolderName { get; set; }
        public int? TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public string GoogleDriveFileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? MimeType { get; set; }
        public int CurrentVersion { get; set; } = 1;
        public Guid UploadedById { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public string UploadedByEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByName { get; set; }
        public int VersionCount { get; set; } = 1;
    }

    public class UpdateFileVersionRequest
    {
        public IFormFile File { get; set; } = null!;
        public string? ChangeNote { get; set; }
    }

    public class RenameProjectFileRequest
    {
        public string FileName { get; set; } = string.Empty;
    }

    public class ProjectFileVersionResponse
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
        public string UploadedByName { get; set; } = string.Empty;
        public string UploadedByEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ProjectFileActivityResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProjectFilesExplorerResponse
    {
        public ProjectFolderResponse? CurrentFolder { get; set; }
        public List<ProjectFolderResponse> Breadcrumbs { get; set; } = new List<ProjectFolderResponse>();
        public List<ProjectFolderResponse> Folders { get; set; } = new List<ProjectFolderResponse>();
        public List<ProjectFileResponse> Files { get; set; } = new List<ProjectFileResponse>();
    }

    public class BatchDownloadRequestDTO
    {
        public List<Guid> FileIds { get; set; } = new List<Guid>();
        public List<Guid> FolderIds { get; set; } = new List<Guid>();
    }

    public class BatchDeleteRequestDTO
    {
        public List<Guid> FileIds { get; set; } = new List<Guid>();
        public List<Guid> FolderIds { get; set; } = new List<Guid>();
    }
}
