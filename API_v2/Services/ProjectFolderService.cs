using API_v2.Exceptions;
using API_v2.Models;
using API_v2.Models.Constants;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class ProjectFolderService : IProjectFolderService
    {
        private readonly IProjectFileRepository _fileRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGoogleDriveService _googleDriveService;
        private readonly ILogger<ProjectFolderService> _logger;

        public ProjectFolderService(
            IProjectFileRepository fileRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IGoogleDriveService googleDriveService,
            ILogger<ProjectFolderService> logger)
        {
            _fileRepository = fileRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _googleDriveService = googleDriveService;
            _logger = logger;
        }

        public async Task<ProjectFilesExplorerResponse> GetExplorerAsync(
            Guid projectId,
            Guid currentUserId,
            Guid? folderId = null,
            int? taskId = null)
        {
            await EnsureProjectMemberAsync(projectId, currentUserId);
            ProjectFolderResponse? currentFolder = null;
            var breadcrumbs = new List<ProjectFolderResponse>();

            if (folderId.HasValue)
            {
                var folder = await GetProjectFolderAsync(projectId, folderId.Value);
                currentFolder = MapFolder(folder);
                breadcrumbs = (await _fileRepository.GetFolderBreadcrumbsAsync(folderId.Value))
                    .Select(folderItem => MapFolder(folderItem))
                    .ToList();
            }

            var folders = await _fileRepository.GetFoldersAsync(projectId, folderId);
            var files = await _fileRepository.GetFilesByFolderIdAsync(projectId, folderId, taskId);
            return new ProjectFilesExplorerResponse
            {
                CurrentFolder = currentFolder,
                Breadcrumbs = breadcrumbs,
                Folders = folders.Select(folder => MapFolder(folder)).ToList(),
                Files = files.Select(MapFile).ToList()
            };
        }

        public async Task<ProjectFolderResponse> CreateFolderAsync(
            Guid projectId,
            Guid currentUserId,
            string name,
            Guid? parentFolderId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw ApiException.BadRequest("Folder name is required.");
            }

            await EnsureProjectMemberAsync(projectId, currentUserId);
            var project = await _projectRepository.GetByIdAsync(projectId)
                ?? throw ApiException.NotFound("Project not found.");
            var parentDriveFolderId = await EnsureProjectDriveFolderAsync(project);

            if (parentFolderId.HasValue)
            {
                var parent = await GetProjectFolderAsync(projectId, parentFolderId.Value);
                if (!string.IsNullOrWhiteSpace(parent.GoogleDriveFolderId))
                {
                    parentDriveFolderId = parent.GoogleDriveFolderId;
                }
            }

            string? driveFolderId = null;
            try
            {
                driveFolderId = await _googleDriveService.CreateFolderAsync(name.Trim(), parentDriveFolderId);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Could not create folder {FolderName} on Google Drive", name);
            }

            var folder = await _fileRepository.AddFolderAsync(new ProjectFolder
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                ParentFolderId = parentFolderId,
                Name = name.Trim(),
                GoogleDriveFolderId = driveFolderId,
                CreatedById = currentUserId,
                CreatedAt = DateTime.UtcNow
            });
            await AddActivityAsync(projectId, currentUserId, "CreateFolder", folder.Name,
                parentFolderId.HasValue ? "Created subfolder" : "Created root folder");

            var user = await _userRepository.GetByIdAsync(currentUserId);
            return MapFolder(folder, user?.FullName ?? user?.Email);
        }

        public async Task<ProjectFolderResponse> RenameFolderAsync(
            Guid projectId,
            Guid folderId,
            Guid currentUserId,
            string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw ApiException.BadRequest("Folder name is required.");
            }

            await EnsureProjectManagerAsync(projectId, currentUserId);
            var folder = await GetProjectFolderAsync(projectId, folderId);
            var oldName = folder.Name;
            folder.Name = newName.Trim();
            await _fileRepository.UpdateFolderAsync(folder);
            await AddActivityAsync(projectId, currentUserId, "RenameFolder", folder.Name,
                $"Renamed folder from '{oldName}' to '{folder.Name}'");
            return MapFolder(folder);
        }

        public async Task DeleteFolderAsync(Guid projectId, Guid folderId, Guid currentUserId)
        {
            await EnsureProjectManagerAsync(projectId, currentUserId);
            var folder = await GetProjectFolderAsync(projectId, folderId);

            if (!string.IsNullOrWhiteSpace(folder.GoogleDriveFolderId))
            {
                try
                {
                    await _googleDriveService.DeleteFileAsync(folder.GoogleDriveFolderId, permanent: false);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Could not trash Google Drive folder {DriveId}", folder.GoogleDriveFolderId);
                }
            }

            await _fileRepository.DeleteFolderAsync(folder);
            await AddActivityAsync(projectId, currentUserId, "DeleteFolder", folder.Name, "Deleted folder");
        }

        private async Task EnsureProjectMemberAsync(Guid projectId, Guid userId)
        {
            if (await _projectRepository.GetMemberAsync(projectId, userId) is null)
            {
                throw ApiException.Forbidden("You do not have access to this project.");
            }
        }

        private async Task EnsureProjectManagerAsync(Guid projectId, Guid userId)
        {
            var member = await _projectRepository.GetMemberAsync(projectId, userId);
            if (member is null || !ProjectRoles.IsOwnerOrManager(member.Role))
            {
                throw ApiException.Forbidden("Only project owners or managers can manage folders.");
            }
        }

        private async Task<ProjectFolder> GetProjectFolderAsync(Guid projectId, Guid folderId)
        {
            var folder = await _fileRepository.GetFolderByIdAsync(folderId);
            return folder is not null && folder.ProjectId == projectId
                ? folder
                : throw ApiException.NotFound("Folder not found.");
        }

        private async Task<string> EnsureProjectDriveFolderAsync(Project project)
        {
            if (!string.IsNullOrWhiteSpace(project.GoogleDriveFolderId)) return project.GoogleDriveFolderId;
            project.GoogleDriveFolderId = await _googleDriveService.CreateFolderAsync($"{project.Name}_{project.Id}");
            await _projectRepository.SaveAsync();
            return project.GoogleDriveFolderId;
        }

        private Task AddActivityAsync(Guid projectId, Guid userId, string action, string target, string details)
            => _fileRepository.AddActivityAsync(new ProjectFileActivity
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = userId,
                ActionType = action,
                TargetName = target,
                Details = details,
                CreatedAt = DateTime.UtcNow
            });

        private static ProjectFolderResponse MapFolder(ProjectFolder folder, string? createdByName = null) => new()
        {
            Id = folder.Id,
            ProjectId = folder.ProjectId,
            ParentFolderId = folder.ParentFolderId,
            Name = folder.Name,
            GoogleDriveFolderId = folder.GoogleDriveFolderId,
            CreatedById = folder.CreatedById,
            CreatedByName = createdByName ?? folder.CreatedBy?.FullName ?? folder.CreatedBy?.Email ?? "Member",
            CreatedAt = folder.CreatedAt,
            FileCount = folder.Files?.Count(file => !file.IsDeleted) ?? 0,
            SubFolderCount = folder.SubFolders?.Count(subfolder => !subfolder.IsDeleted) ?? 0
        };

        private static ProjectFileResponse MapFile(ProjectFile file) => new()
        {
            Id = file.Id,
            ProjectId = file.ProjectId,
            FolderId = file.FolderId,
            FolderName = file.Folder?.Name,
            TaskId = file.TaskId,
            TaskTitle = file.Task?.Title,
            GoogleDriveFileId = file.GoogleDriveFileId,
            FileName = file.FileName,
            FileSize = file.FileSize,
            MimeType = file.MimeType,
            CurrentVersion = file.CurrentVersion,
            UploadedById = file.UploadedById,
            UploadedByName = file.UploadedBy?.FullName ?? file.UploadedBy?.Email ?? "Member",
            UploadedByEmail = file.UploadedBy?.Email ?? string.Empty,
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt,
            UpdatedByName = file.UpdatedBy?.FullName ?? file.UpdatedBy?.Email,
            VersionCount = file.Versions?.Count > 0 ? file.Versions.Count : Math.Max(file.CurrentVersion, 1)
        };
    }
}
