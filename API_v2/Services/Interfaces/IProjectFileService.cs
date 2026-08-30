using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface IProjectFileService
    {
        // Explorer & Files
        Task<List<ProjectFileResponse>> GetFilesAsync(Guid projectId, Guid currentUserId, Guid? folderId = null, int? taskId = null);
        Task<ProjectFileResponse> UploadFileAsync(Guid projectId, Guid currentUserId, Stream fileStream,
            string fileName, string contentType, long fileSize, Guid? folderId = null, int? taskId = null);
        Task<ProjectFileResponse> RenameFileAsync(Guid projectId, Guid fileId, Guid currentUserId, string newFileName);
        Task DeleteFileAsync(Guid projectId, Guid fileId, Guid currentUserId);
        Task DeleteMultipleAsync(Guid projectId, Guid currentUserId, List<Guid> fileIds, List<Guid>? folderIds = null);

        // Activities
        Task<List<ProjectFileActivityResponse>> GetActivitiesAsync(Guid projectId, Guid currentUserId);
    }
}
