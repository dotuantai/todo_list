using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories
{
    public class ProjectFileRepository : IProjectFileRepository
    {
        private readonly AppDbContext _dbContext;

        public ProjectFileRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ==================== FILES ====================

        public async Task<List<ProjectFile>> GetFilesByFolderIdAsync(Guid projectId, Guid? folderId, int? taskId = null)
        {
            var query = _dbContext.ProjectFiles
                .AsNoTracking()
                .Include(pf => pf.UploadedBy)
                .Include(pf => pf.UpdatedBy)
                .Include(pf => pf.Folder)
                .Include(pf => pf.Task)
                .Include(pf => pf.Versions)
                .Where(pf => pf.ProjectId == projectId && pf.FolderId == folderId && !pf.IsDeleted);

            if (taskId.HasValue)
            {
                query = query.Where(pf => pf.TaskId == taskId.Value);
            }

            return await query
                .OrderByDescending(pf => pf.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ProjectFile>> GetFilesByProjectIdAsync(Guid projectId, int? taskId = null)
        {
            var query = _dbContext.ProjectFiles
                .AsNoTracking()
                .Include(pf => pf.UploadedBy)
                .Include(pf => pf.UpdatedBy)
                .Include(pf => pf.Folder)
                .Include(pf => pf.Task)
                .Include(pf => pf.Versions)
                .Where(pf => pf.ProjectId == projectId && !pf.IsDeleted);

            if (taskId.HasValue)
            {
                query = query.Where(pf => pf.TaskId == taskId.Value);
            }

            return await query
                .OrderByDescending(pf => pf.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProjectFile?> GetFileByIdAsync(Guid fileId)
        {
            return await _dbContext.ProjectFiles
                .Include(pf => pf.UploadedBy)
                .Include(pf => pf.UpdatedBy)
                .Include(pf => pf.Folder)
                .Include(pf => pf.Task)
                .Include(pf => pf.Project)
                .Include(pf => pf.Versions)
                .FirstOrDefaultAsync(pf => pf.Id == fileId && !pf.IsDeleted);
        }

        public async Task<ProjectFile?> GetFileByNameAsync(Guid projectId, Guid? folderId, string fileName)
        {
            return await _dbContext.ProjectFiles
                .Include(pf => pf.UploadedBy)
                .Include(pf => pf.UpdatedBy)
                .Include(pf => pf.Folder)
                .Include(pf => pf.Task)
                .Include(pf => pf.Project)
                .Include(pf => pf.Versions)
                .FirstOrDefaultAsync(pf => pf.ProjectId == projectId && pf.FolderId == folderId && pf.FileName.ToLower() == fileName.ToLower() && !pf.IsDeleted);
        }

        public async Task<ProjectFile> AddFileAsync(ProjectFile file)
        {
            var entry = await _dbContext.ProjectFiles.AddAsync(file);
            await _dbContext.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task UpdateFileAsync(ProjectFile file)
        {
            _dbContext.ProjectFiles.Update(file);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteFileAsync(ProjectFile file)
        {
            file.IsDeleted = true;
            _dbContext.ProjectFiles.Update(file);
            await _dbContext.SaveChangesAsync();
        }

        // ==================== FOLDERS ====================

        public async Task<List<ProjectFolder>> GetFoldersAsync(Guid projectId, Guid? parentFolderId)
        {
            return await _dbContext.ProjectFolders
                .AsNoTracking()
                .Include(f => f.CreatedBy)
                .Include(f => f.Files.Where(file => !file.IsDeleted))
                .Include(f => f.SubFolders.Where(sub => !sub.IsDeleted))
                .Where(f => f.ProjectId == projectId && f.ParentFolderId == parentFolderId && !f.IsDeleted)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<ProjectFolder?> GetFolderByIdAsync(Guid folderId)
        {
            return await _dbContext.ProjectFolders
                .Include(f => f.CreatedBy)
                .Include(f => f.ParentFolder)
                .Include(f => f.Files.Where(file => !file.IsDeleted))
                .Include(f => f.SubFolders.Where(sub => !sub.IsDeleted))
                .FirstOrDefaultAsync(f => f.Id == folderId && !f.IsDeleted);
        }

        public async Task<ProjectFolder> AddFolderAsync(ProjectFolder folder)
        {
            var entry = await _dbContext.ProjectFolders.AddAsync(folder);
            await _dbContext.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task UpdateFolderAsync(ProjectFolder folder)
        {
            _dbContext.ProjectFolders.Update(folder);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteFolderAsync(ProjectFolder folder)
        {
            folder.IsDeleted = true;
            _dbContext.ProjectFolders.Update(folder);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ProjectFolder>> GetFolderBreadcrumbsAsync(Guid folderId)
        {
            var breadcrumbs = new List<ProjectFolder>();
            var currentId = (Guid?)folderId;

            while (currentId.HasValue)
            {
                var folder = await _dbContext.ProjectFolders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.Id == currentId.Value && !f.IsDeleted);

                if (folder == null) break;

                breadcrumbs.Insert(0, folder);
                currentId = folder.ParentFolderId;
            }

            return breadcrumbs;
        }

        public async Task<List<ProjectFile>> GetAllFilesInFolderHierarchyAsync(Guid projectId, Guid folderId)
        {
            var allFolderIds = new HashSet<Guid> { folderId };
            var queue = new Queue<Guid>();
            queue.Enqueue(folderId);

            var allProjectFolders = await _dbContext.ProjectFolders
                .AsNoTracking()
                .Where(f => f.ProjectId == projectId && !f.IsDeleted)
                .ToListAsync();

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var children = allProjectFolders.Where(f => f.ParentFolderId == currentId).ToList();
                foreach (var child in children)
                {
                    if (allFolderIds.Add(child.Id))
                    {
                        queue.Enqueue(child.Id);
                    }
                }
            }

            return await _dbContext.ProjectFiles
                .AsNoTracking()
                .Include(f => f.Folder)
                .Include(f => f.Versions)
                .Where(f => f.ProjectId == projectId && f.FolderId != null && allFolderIds.Contains(f.FolderId.Value) && !f.IsDeleted)
                .ToListAsync();
        }

        // ==================== VERSIONS ====================

        public async Task<ProjectFileVersion> AddFileVersionAsync(ProjectFileVersion version)
        {
            var entry = await _dbContext.ProjectFileVersions.AddAsync(version);
            await _dbContext.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<List<ProjectFileVersion>> GetFileVersionsAsync(Guid fileId)
        {
            return await _dbContext.ProjectFileVersions
                .AsNoTracking()
                .Include(v => v.UploadedBy)
                .Where(v => v.ProjectFileId == fileId)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();
        }

        public async Task<ProjectFileVersion?> GetFileVersionByIdAsync(Guid versionId)
        {
            return await _dbContext.ProjectFileVersions
                .Include(v => v.UploadedBy)
                .Include(v => v.ProjectFile)
                .FirstOrDefaultAsync(v => v.Id == versionId);
        }

        // ==================== ACTIVITIES ====================

        public async Task AddActivityAsync(ProjectFileActivity activity)
        {
            await _dbContext.ProjectFileActivities.AddAsync(activity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ProjectFileActivity>> GetActivitiesByProjectIdAsync(Guid projectId, int limit = 50)
        {
            return await _dbContext.ProjectFileActivities
                .AsNoTracking()
                .Include(a => a.User)
                .Where(a => a.ProjectId == projectId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}
