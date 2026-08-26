using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface IProjectFolderService
    {
        Task<ProjectFilesExplorerResponse> GetExplorerAsync(Guid projectId, Guid currentUserId, Guid? folderId = null, int? taskId = null);
        Task<ProjectFolderResponse> CreateFolderAsync(Guid projectId, Guid currentUserId, string name, Guid? parentFolderId = null);
        Task<ProjectFolderResponse> RenameFolderAsync(Guid projectId, Guid folderId, Guid currentUserId, string newName);
        Task DeleteFolderAsync(Guid projectId, Guid folderId, Guid currentUserId);
    }
}
