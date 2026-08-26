using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using API_v2.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace API_v2.Services.Interfaces
{
    public interface IProjectFileService
    {
        // Explorer & Files
        Task<ProjectFilesExplorerResponse> GetExplorerAsync(Guid projectId, Guid currentUserId, Guid? folderId = null, int? taskId = null);
        Task<List<ProjectFileResponse>> GetFilesAsync(Guid projectId, Guid currentUserId, Guid? folderId = null, int? taskId = null);
        Task<ProjectFileResponse> UploadFileAsync(Guid projectId, Guid currentUserId, IFormFile file, Guid? folderId = null, int? taskId = null);
        Task<ProjectFileResponse> UpdateFileVersionAsync(Guid projectId, Guid fileId, Guid currentUserId, IFormFile file, string? changeNote = null);
        Task<List<ProjectFileVersionResponse>> GetFileVersionsAsync(Guid projectId, Guid fileId, Guid currentUserId);
        Task<(Stream Stream, string MimeType, string FileName)> DownloadFileAsync(Guid projectId, Guid fileId, Guid currentUserId, Guid? versionId = null);
        Task<(Stream Stream, string MimeType, string FileName)> DownloadMultipleFilesAsync(Guid projectId, Guid currentUserId, List<Guid> fileIds, List<Guid>? folderIds = null);
        Task<ProjectFileResponse> RenameFileAsync(Guid projectId, Guid fileId, Guid currentUserId, string newFileName);
        Task DeleteFileAsync(Guid projectId, Guid fileId, Guid currentUserId);
        Task DeleteMultipleAsync(Guid projectId, Guid currentUserId, List<Guid> fileIds, List<Guid>? folderIds = null);

        // Folders
        Task<ProjectFolderResponse> CreateFolderAsync(Guid projectId, Guid currentUserId, string name, Guid? parentFolderId = null);
        Task<ProjectFolderResponse> RenameFolderAsync(Guid projectId, Guid folderId, Guid currentUserId, string newName);
        Task DeleteFolderAsync(Guid projectId, Guid folderId, Guid currentUserId);

        // Activities
        Task<List<ProjectFileActivityResponse>> GetActivitiesAsync(Guid projectId, Guid currentUserId);
    }
}
