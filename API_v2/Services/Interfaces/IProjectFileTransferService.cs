using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface IProjectFileTransferService
    {
        Task<ProjectFileResponse> UpdateFileVersionAsync(Guid projectId, Guid fileId, Guid currentUserId,
            Stream fileStream, string fileName, string contentType, long fileSize, string? changeNote = null);
        Task<List<ProjectFileVersionResponse>> GetFileVersionsAsync(Guid projectId, Guid fileId, Guid currentUserId);
        Task<(Stream Stream, string MimeType, string FileName)> DownloadFileAsync(Guid projectId, Guid fileId, Guid currentUserId, Guid? versionId = null);
        Task<(Stream Stream, string MimeType, string FileName)> DownloadMultipleFilesAsync(Guid projectId, Guid currentUserId, List<Guid> fileIds, List<Guid>? folderIds = null);
    }
}
