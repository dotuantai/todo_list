using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models;

namespace API_v2.Repositories.IRepositories
{
    public interface IProjectFileRepository
    {
        // Files
        Task<List<ProjectFile>> GetFilesByFolderIdAsync(Guid projectId, Guid? folderId, int? taskId = null);
        Task<List<ProjectFile>> GetFilesByProjectIdAsync(Guid projectId, int? taskId = null);
        Task<ProjectFile?> GetFileByIdAsync(Guid fileId);
        Task<ProjectFile?> GetFileByNameAsync(Guid projectId, Guid? folderId, string fileName);
        Task<ProjectFile> AddFileAsync(ProjectFile file);
        Task UpdateFileAsync(ProjectFile file);
        Task DeleteFileAsync(ProjectFile file);

        // Folders
        Task<List<ProjectFolder>> GetFoldersAsync(Guid projectId, Guid? parentFolderId);
        Task<ProjectFolder?> GetFolderByIdAsync(Guid folderId);
        Task<ProjectFolder> AddFolderAsync(ProjectFolder folder);
        Task UpdateFolderAsync(ProjectFolder folder);
        Task DeleteFolderAsync(ProjectFolder folder);
        Task<List<ProjectFolder>> GetFolderBreadcrumbsAsync(Guid folderId);
        Task<List<ProjectFile>> GetAllFilesInFolderHierarchyAsync(Guid projectId, Guid folderId);

        // Versions
        Task<ProjectFileVersion> AddFileVersionAsync(ProjectFileVersion version);
        Task<List<ProjectFileVersion>> GetFileVersionsAsync(Guid fileId);
        Task<ProjectFileVersion?> GetFileVersionByIdAsync(Guid versionId);

        // Activities
        Task AddActivityAsync(ProjectFileActivity activity);
        Task<List<ProjectFileActivity>> GetActivitiesByProjectIdAsync(Guid projectId, int limit = 50);
    }
}
