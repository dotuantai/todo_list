using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Exceptions;
using API_v2.Models;
using API_v2.Models.Constants;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace API_v2.Services
{
    public class ProjectFileService : IProjectFileService
    {
        private readonly IProjectFileRepository _fileRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IUserRepository _userRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IGoogleDriveService _googleDriveService;
        private readonly IProjectFileTransferService _transferService;
        private readonly ILogger<ProjectFileService> _logger;

        public ProjectFileService(
            IProjectFileRepository fileRepo,
            IProjectRepository projectRepo,
            IUserRepository userRepo,
            ITaskRepository taskRepo,
            IGoogleDriveService googleDriveService,
            IProjectFileTransferService transferService,
            ILogger<ProjectFileService> logger)
        {
            _fileRepo = fileRepo;
            _projectRepo = projectRepo;
            _userRepo = userRepo;
            _taskRepo = taskRepo;
            _googleDriveService = googleDriveService;
            _transferService = transferService;
            _logger = logger;
        }

        // ==================== EXPLORER & FILES ====================

        public async Task<List<ProjectFileResponse>> GetFilesAsync(Guid projectId, Guid currentUserId, Guid? folderId = null, int? taskId = null)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null && !await _projectRepo.IsSystemAdminAsync(currentUserId))
            {
                throw ApiException.Forbidden("Bạn không có quyền truy cập vào dự án này.");
            }

            var files = folderId.HasValue
                ? await _fileRepo.GetFilesByFolderIdAsync(projectId, folderId.Value, taskId)
                : await _fileRepo.GetFilesByProjectIdAsync(projectId, taskId);

            return files.Select(file => MapToFileResponse(file, null, null, null)).ToList();
        }

        public async Task<ProjectFileResponse> UploadFileAsync(Guid projectId, Guid currentUserId, Stream fileStream,
            string fileName, string contentType, long fileSize, Guid? folderId = null, int? taskId = null)
        {
            if (fileStream == null || !fileStream.CanRead || fileSize <= 0)
            {
                throw ApiException.BadRequest("Vui lòng chọn tệp hợp lệ để tải lên.");
            }

            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null)
            {
                throw ApiException.NotFound("Dự án không tồn tại.");
            }

            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null && !await _projectRepo.IsSystemAdminAsync(currentUserId))
            {
                throw ApiException.Forbidden("Bạn không có quyền tải tệp lên dự án này.");
            }

            if (taskId.HasValue)
            {
                var linkedTask = await _taskRepo.GetByIdAsync(taskId.Value);
                if (linkedTask == null || linkedTask.ProjectId != projectId)
                {
                    throw ApiException.BadRequest("Công việc được liên kết không tồn tại hoặc không thuộc dự án này.");
                }
            }

            var user = await _userRepo.GetByIdAsync(currentUserId);
            if (user == null)
            {
                throw ApiException.Unauthorized("Thông tin tài khoản không hợp lệ.");
            }

            // Check if a file with the same name already exists in the same folder
            var existingFile = await _fileRepo.GetFileByNameAsync(projectId, folderId, fileName);
            if (existingFile != null)
            {
                _logger.LogInformation("File '{FileName}' already exists in folder {FolderId}. Automatically updating to version {NextVersion}", fileName, folderId, existingFile.CurrentVersion + 1);
                return await _transferService.UpdateFileVersionAsync(
                    projectId, 
                    existingFile.Id, 
                    currentUserId, 
                    fileStream, fileName, contentType, fileSize,
                    "Tự động nâng phiên bản khi tải lên tệp cùng tên"
                );
            }

            // Ensure project has root Google Drive folder
            var projectDriveFolderId = await EnsureProjectDriveFolderAsync(project);

            // Determine target Google Drive folder
            string targetDriveFolderId = projectDriveFolderId;
            ProjectFolder? targetFolder = null;
            if (folderId.HasValue)
            {
                targetFolder = await _fileRepo.GetFolderByIdAsync(folderId.Value);
                if (targetFolder == null || targetFolder.ProjectId != projectId)
                {
                    throw ApiException.NotFound("Thư mục đích không tồn tại.");
                }

                if (!string.IsNullOrWhiteSpace(targetFolder.GoogleDriveFolderId))
                {
                    targetDriveFolderId = targetFolder.GoogleDriveFolderId;
                }
            }

            string googleDriveFileId;
            try
            {
                var uploadResult = await _googleDriveService.UploadFileAsync(
                    fileStream,
                    fileName,
                    contentType,
                    targetDriveFolderId
                );
                googleDriveFileId = uploadResult.FileId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file '{FileName}' to Google Drive for project {ProjectId}", fileName, projectId);
                throw ApiException.InternalServerError($"Lỗi tải file lên Google Drive: {ex.Message}");
            }

            var projectFile = new ProjectFile
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                FolderId = folderId,
                TaskId = taskId,
                GoogleDriveFileId = googleDriveFileId,
                FileName = fileName,
                FileSize = fileSize,
                MimeType = contentType,
                CurrentVersion = 1,
                UploadedById = currentUserId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var savedFile = await _fileRepo.AddFileAsync(projectFile);

            // Create initial version (v1)
            var initialVersion = new ProjectFileVersion
            {
                Id = Guid.NewGuid(),
                ProjectFileId = savedFile.Id,
                VersionNumber = 1,
                GoogleDriveFileId = googleDriveFileId,
                FileName = fileName,
                FileSize = fileSize,
                MimeType = contentType,
                ChangeNote = "Phiên bản khởi tạo",
                UploadedById = currentUserId,
                CreatedAt = DateTime.UtcNow
            };
            await _fileRepo.AddFileVersionAsync(initialVersion);

            // Record activity
            await _fileRepo.AddActivityAsync(new ProjectFileActivity
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = currentUserId,
                ActionType = "UploadFile",
                TargetName = fileName,
                Details = targetFolder != null ? $"Tải tệp vào thư mục '{targetFolder.Name}'" : "Tải tệp vào thư mục gốc",
                CreatedAt = DateTime.UtcNow
            });

            return MapToFileResponse(savedFile, user.FullName ?? user.Email, user.Email);
        }

        public async Task DeleteMultipleAsync(Guid projectId, Guid currentUserId, List<Guid> fileIds, List<Guid>? folderIds = null)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            var isAdmin = await _projectRepo.IsSystemAdminAsync(currentUserId);
            if (member == null && !isAdmin)
            {
                throw ApiException.Forbidden("Bạn không có quyền thao tác trên dự án này.");
            }

            bool isPrivileged = isAdmin || ProjectRoles.IsOwnerOrManager(member!.Role);
            if (!isPrivileged)
            {
                throw ApiException.Forbidden("Chỉ Quản lý hoặc Quản trị viên mới có quyền xóa hàng loạt.");
            }

            fileIds ??= new List<Guid>();
            folderIds ??= new List<Guid>();

            int deletedCount = 0;

            foreach (var fileId in fileIds.Distinct())
            {
                var file = await _fileRepo.GetFileByIdAsync(fileId);
                if (file != null && file.ProjectId == projectId && !file.IsDeleted)
                {
                    await _fileRepo.DeleteFileAsync(file);
                    try { await _googleDriveService.DeleteFileAsync(file.GoogleDriveFileId); } catch { }
                    deletedCount++;
                }
            }

            foreach (var folderId in folderIds.Distinct())
            {
                var folder = await _fileRepo.GetFolderByIdAsync(folderId);
                if (folder != null && folder.ProjectId == projectId && !folder.IsDeleted)
                {
                    await _fileRepo.DeleteFolderAsync(folder);
                    if (!string.IsNullOrWhiteSpace(folder.GoogleDriveFolderId))
                    {
                        try { await _googleDriveService.DeleteFileAsync(folder.GoogleDriveFolderId); } catch { }
                    }
                    deletedCount++;
                }
            }

            if (deletedCount > 0)
            {
                await _fileRepo.AddActivityAsync(new ProjectFileActivity
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    UserId = currentUserId,
                    ActionType = "DeleteFile",
                    TargetName = $"{deletedCount} mục",
                    Details = $"Đã xóa hàng loạt {deletedCount} tệp/thư mục khỏi dự án",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        public async Task<ProjectFileResponse> RenameFileAsync(Guid projectId, Guid fileId, Guid currentUserId, string newFileName)
        {
            if (string.IsNullOrWhiteSpace(newFileName))
            {
                throw ApiException.BadRequest("Tên tệp không được để trống.");
            }

            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null && !await _projectRepo.IsSystemAdminAsync(currentUserId))
            {
                throw ApiException.Forbidden("Bạn không có quyền thao tác trên dự án này.");
            }

            var file = await _fileRepo.GetFileByIdAsync(fileId);
            if (file == null || file.ProjectId != projectId)
            {
                throw ApiException.NotFound("Tài liệu không tồn tại.");
            }

            var oldName = file.FileName;
            file.FileName = newFileName.Trim();
            file.UpdatedAt = DateTime.UtcNow;
            file.UpdatedById = currentUserId;
            await _fileRepo.UpdateFileAsync(file);

            await _fileRepo.AddActivityAsync(new ProjectFileActivity
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = currentUserId,
                ActionType = "RenameFile",
                TargetName = file.FileName,
                Details = $"Đổi tên từ '{oldName}' thành '{file.FileName}'",
                CreatedAt = DateTime.UtcNow
            });

            return MapToFileResponse(file);
        }

        public async Task DeleteFileAsync(Guid projectId, Guid fileId, Guid currentUserId)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            var isAdmin = await _projectRepo.IsSystemAdminAsync(currentUserId);
            if (member == null && !isAdmin)
            {
                throw ApiException.Forbidden("Bạn không có quyền truy cập vào dự án này.");
            }

            if (!isAdmin && !ProjectRoles.IsOwnerOrManager(member!.Role))
            {
                throw ApiException.Forbidden("Chỉ Quản lý dự án (Manager) trở lên mới có quyền xóa tài liệu.");
            }

            var file = await _fileRepo.GetFileByIdAsync(fileId);
            if (file == null || file.ProjectId != projectId)
            {
                throw ApiException.NotFound("Tài liệu không tồn tại hoặc đã bị xóa.");
            }

            try
            {
                await _googleDriveService.DeleteFileAsync(file.GoogleDriveFileId, permanent: false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not move file {DriveId} to trash on Google Drive, proceeding with database delete.", file.GoogleDriveFileId);
            }

            await _fileRepo.DeleteFileAsync(file);

            await _fileRepo.AddActivityAsync(new ProjectFileActivity
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = currentUserId,
                ActionType = "DeleteFile",
                TargetName = file.FileName,
                Details = "Đã xóa tài liệu khỏi dự án",
                CreatedAt = DateTime.UtcNow
            });
        }

        // ==================== ACTIVITIES ====================

        public async Task<List<ProjectFileActivityResponse>> GetActivitiesAsync(Guid projectId, Guid currentUserId)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null && !await _projectRepo.IsSystemAdminAsync(currentUserId))
            {
                throw ApiException.Forbidden("Bạn không có quyền truy cập vào dự án này.");
            }

            var activities = await _fileRepo.GetActivitiesByProjectIdAsync(projectId, 50);
            return activities.Select(a => new ProjectFileActivityResponse
            {
                Id = a.Id,
                ProjectId = a.ProjectId,
                UserId = a.UserId,
                UserName = a.User != null && !string.IsNullOrWhiteSpace(a.User.FullName) ? a.User.FullName : (a.User?.Email ?? "Người dùng"),
                UserEmail = a.User?.Email ?? "",
                ActionType = a.ActionType,
                TargetName = a.TargetName,
                Details = a.Details,
                CreatedAt = a.CreatedAt
            }).ToList();
        }

        // ==================== HELPERS ====================

        private async Task<string> EnsureProjectDriveFolderAsync(Project project)
        {
            if (!string.IsNullOrWhiteSpace(project.GoogleDriveFolderId))
            {
                return project.GoogleDriveFolderId;
            }

            var folderName = $"{project.Name}_{project.Id}";
            var folderId = await _googleDriveService.CreateFolderAsync(folderName);
            project.GoogleDriveFolderId = folderId;
            await _projectRepo.SaveAsync();
            return folderId;
        }

        private static ProjectFileResponse MapToFileResponse(ProjectFile f, string? uploadedByName = null, string? uploadedByEmail = null, string? updatedByName = null)
        {
            return new ProjectFileResponse
            {
                Id = f.Id,
                ProjectId = f.ProjectId,
                FolderId = f.FolderId,
                FolderName = f.Folder?.Name,
                TaskId = f.TaskId,
                TaskTitle = f.Task?.Title,
                GoogleDriveFileId = f.GoogleDriveFileId,
                FileName = f.FileName,
                FileSize = f.FileSize,
                MimeType = f.MimeType,
                CurrentVersion = f.CurrentVersion,
                UploadedById = f.UploadedById,
                UploadedByName = uploadedByName ?? (f.UploadedBy != null && !string.IsNullOrWhiteSpace(f.UploadedBy.FullName) ? f.UploadedBy.FullName : (f.UploadedBy?.Email ?? "Thành viên")),
                UploadedByEmail = uploadedByEmail ?? (f.UploadedBy?.Email ?? ""),
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt,
                UpdatedByName = updatedByName ?? (f.UpdatedBy != null && !string.IsNullOrWhiteSpace(f.UpdatedBy.FullName) ? f.UpdatedBy.FullName : f.UpdatedBy?.Email),
                VersionCount = f.Versions?.Count > 0 ? f.Versions.Count : (f.CurrentVersion > 0 ? f.CurrentVersion : 1)
            };
        }
    }
}
