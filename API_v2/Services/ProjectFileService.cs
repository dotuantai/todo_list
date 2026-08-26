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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace API_v2.Services
{
    public class ProjectFileService : IProjectFileService
    {
        private readonly IProjectFileRepository _fileRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IUserRepository _userRepo;
        private readonly IGoogleDriveService _googleDriveService;
        private readonly ILogger<ProjectFileService> _logger;

        public ProjectFileService(
            IProjectFileRepository fileRepo,
            IProjectRepository projectRepo,
            IUserRepository userRepo,
            IGoogleDriveService googleDriveService,
            ILogger<ProjectFileService> logger)
        {
            _fileRepo = fileRepo;
            _projectRepo = projectRepo;
            _userRepo = userRepo;
            _googleDriveService = googleDriveService;
            _logger = logger;
        }

        // ==================== EXPLORER & FILES ====================

        public async Task<ProjectFilesExplorerResponse> GetExplorerAsync(Guid projectId, Guid currentUserId, Guid? folderId = null, int? taskId = null)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null)
            {
                throw ApiException.Forbidden("Bạn không có quyền truy cập vào dự án này.");
            }

            ProjectFolderResponse? currentFolder = null;
            var breadcrumbs = new List<ProjectFolderResponse>();

            if (folderId.HasValue)
            {
                var folderEntity = await _fileRepo.GetFolderByIdAsync(folderId.Value);
                if (folderEntity == null || folderEntity.ProjectId != projectId)
                {
                    throw ApiException.NotFound("Thư mục không tồn tại.");
                }

                currentFolder = MapToFolderResponse(folderEntity);

                var breadcrumbEntities = await _fileRepo.GetFolderBreadcrumbsAsync(folderId.Value);
                breadcrumbs = breadcrumbEntities.Select(b => MapToFolderResponse(b, null)).ToList();
            }

            var subFolders = await _fileRepo.GetFoldersAsync(projectId, folderId);
            var files = await _fileRepo.GetFilesByFolderIdAsync(projectId, folderId, taskId);

            return new ProjectFilesExplorerResponse
            {
                CurrentFolder = currentFolder,
                Breadcrumbs = breadcrumbs,
                Folders = subFolders.Select(sf => MapToFolderResponse(sf, null)).ToList(),
                Files = files.Select(file => MapToFileResponse(file, null, null, null)).ToList()
            };
        }

        public async Task<List<ProjectFileResponse>> GetFilesAsync(Guid projectId, Guid currentUserId, Guid? folderId = null, int? taskId = null)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null)
            {
                throw ApiException.Forbidden("Bạn không có quyền truy cập vào dự án này.");
            }

            var files = folderId.HasValue
                ? await _fileRepo.GetFilesByFolderIdAsync(projectId, folderId.Value, taskId)
                : await _fileRepo.GetFilesByProjectIdAsync(projectId, taskId);

            return files.Select(file => MapToFileResponse(file, null, null, null)).ToList();
        }

        public async Task<ProjectFileResponse> UploadFileAsync(Guid projectId, Guid currentUserId, IFormFile file, Guid? folderId = null, int? taskId = null)
        {
            if (file == null || file.Length == 0)
            {
                throw ApiException.BadRequest("Vui lòng chọn tệp hợp lệ để tải lên.");
            }

            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null)
            {
                throw ApiException.NotFound("Dự án không tồn tại.");
            }

            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null)
            {
                throw ApiException.Forbidden("Bạn không có quyền tải tệp lên dự án này.");
            }

            var user = await _userRepo.GetByIdAsync(currentUserId);
            if (user == null)
            {
                throw ApiException.Unauthorized("Thông tin tài khoản không hợp lệ.");
            }

            // Check if a file with the same name already exists in the same folder
            var existingFile = await _fileRepo.GetFileByNameAsync(projectId, folderId, file.FileName);
            if (existingFile != null)
            {
                _logger.LogInformation("File '{FileName}' already exists in folder {FolderId}. Automatically updating to version {NextVersion}", file.FileName, folderId, existingFile.CurrentVersion + 1);
                return await UpdateFileVersionAsync(
                    projectId, 
                    existingFile.Id, 
                    currentUserId, 
                    file, 
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
                using var stream = file.OpenReadStream();
                var uploadResult = await _googleDriveService.UploadFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    targetDriveFolderId
                );
                googleDriveFileId = uploadResult.FileId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file '{FileName}' to Google Drive for project {ProjectId}", file.FileName, projectId);
                throw ApiException.InternalServerError($"Lỗi tải file lên Google Drive: {ex.Message}");
            }

            var projectFile = new ProjectFile
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                FolderId = folderId,
                TaskId = taskId,
                GoogleDriveFileId = googleDriveFileId,
                FileName = file.FileName,
                FileSize = file.Length,
                MimeType = file.ContentType,
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
                FileName = file.FileName,
                FileSize = file.Length,
                MimeType = file.ContentType,
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
                TargetName = file.FileName,
                Details = targetFolder != null ? $"Tải tệp vào thư mục '{targetFolder.Name}'" : "Tải tệp vào thư mục gốc",
                CreatedAt = DateTime.UtcNow
            });

            return MapToFileResponse(savedFile, user.FullName ?? user.Email, user.Email);
        }

        public async Task<ProjectFileResponse> UpdateFileVersionAsync(Guid projectId, Guid fileId, Guid currentUserId, IFormFile file, string? changeNote = null)
        {
            if (file == null || file.Length == 0)
            {
                throw ApiException.BadRequest("Vui lòng chọn tệp hợp lệ để cập nhật.");
            }

            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null)
            {
                throw ApiException.Forbidden("Bạn không có quyền cập nhật tài liệu này.");
            }

            var projectFile = await _fileRepo.GetFileByIdAsync(fileId);
            if (projectFile == null || projectFile.ProjectId != projectId)
            {
                throw ApiException.NotFound("Tài liệu không tồn tại.");
            }

            var project = await _projectRepo.GetByIdAsync(projectId);
            var projectDriveFolderId = await EnsureProjectDriveFolderAsync(project!);

            string targetDriveFolderId = projectDriveFolderId;
            if (projectFile.FolderId.HasValue && projectFile.Folder != null && !string.IsNullOrWhiteSpace(projectFile.Folder.GoogleDriveFolderId))
            {
                targetDriveFolderId = projectFile.Folder.GoogleDriveFolderId;
            }

            string newGoogleDriveFileId;
            try
            {
                using var stream = file.OpenReadStream();
                var uploadResult = await _googleDriveService.UploadFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    targetDriveFolderId
                );
                newGoogleDriveFileId = uploadResult.FileId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload new version of file '{FileId}'", fileId);
                throw ApiException.InternalServerError($"Lỗi tải phiên bản mới lên Google Drive: {ex.Message}");
            }

            var newVersionNumber = projectFile.CurrentVersion + 1;

            // Add new version entry
            var versionEntry = new ProjectFileVersion
            {
                Id = Guid.NewGuid(),
                ProjectFileId = projectFile.Id,
                VersionNumber = newVersionNumber,
                GoogleDriveFileId = newGoogleDriveFileId,
                FileName = file.FileName,
                FileSize = file.Length,
                MimeType = file.ContentType,
                ChangeNote = !string.IsNullOrWhiteSpace(changeNote) ? changeNote : $"Cập nhật phiên bản {newVersionNumber}",
                UploadedById = currentUserId,
                CreatedAt = DateTime.UtcNow
            };
            await _fileRepo.AddFileVersionAsync(versionEntry);

            // Update main file record
            projectFile.GoogleDriveFileId = newGoogleDriveFileId;
            projectFile.FileName = file.FileName;
            projectFile.FileSize = file.Length;
            projectFile.MimeType = file.ContentType;
            projectFile.CurrentVersion = newVersionNumber;
            projectFile.UpdatedAt = DateTime.UtcNow;
            projectFile.UpdatedById = currentUserId;
            await _fileRepo.UpdateFileAsync(projectFile);

            var user = await _userRepo.GetByIdAsync(currentUserId);

            // Record activity
            await _fileRepo.AddActivityAsync(new ProjectFileActivity
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = currentUserId,
                ActionType = "UpdateVersion",
                TargetName = file.FileName,
                Details = $"Cập nhật phiên bản v{newVersionNumber}" + (!string.IsNullOrWhiteSpace(changeNote) ? $": {changeNote}" : ""),
                CreatedAt = DateTime.UtcNow
            });

            return MapToFileResponse(projectFile, projectFile.UploadedBy?.FullName ?? projectFile.UploadedBy?.Email ?? "Thành viên", projectFile.UploadedBy?.Email ?? "", user?.FullName ?? user?.Email);
        }

        public async Task<List<ProjectFileVersionResponse>> GetFileVersionsAsync(Guid projectId, Guid fileId, Guid currentUserId)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null)
            {
                throw ApiException.Forbidden("Bạn không có quyền truy cập tài liệu này.");
            }

            var file = await _fileRepo.GetFileByIdAsync(fileId);
            if (file == null || file.ProjectId != projectId)
            {
                throw ApiException.NotFound("Tài liệu không tồn tại.");
            }

            var versions = await _fileRepo.GetFileVersionsAsync(fileId);
            return versions.Select(v => new ProjectFileVersionResponse
            {
                Id = v.Id,
                ProjectFileId = v.ProjectFileId,
                VersionNumber = v.VersionNumber,
                GoogleDriveFileId = v.GoogleDriveFileId,
                FileName = v.FileName,
                FileSize = v.FileSize,
                MimeType = v.MimeType,
                ChangeNote = v.ChangeNote,
                UploadedById = v.UploadedById,
                UploadedByName = v.UploadedBy != null && !string.IsNullOrWhiteSpace(v.UploadedBy.FullName) ? v.UploadedBy.FullName : (v.UploadedBy?.Email ?? "Thành viên"),
                UploadedByEmail = v.UploadedBy?.Email ?? "",
                CreatedAt = v.CreatedAt
            }).ToList();
        }

        public async Task<(Stream Stream, string MimeType, string FileName)> DownloadFileAsync(Guid projectId, Guid fileId, Guid currentUserId, Guid? versionId = null)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null)
            {
                throw ApiException.Forbidden("Bạn không có quyền truy cập tài liệu này.");
            }

            var file = await _fileRepo.GetFileByIdAsync(fileId);
            if (file == null || file.ProjectId != projectId)
            {
                throw ApiException.NotFound("Tài liệu không tồn tại hoặc đã bị xóa.");
            }

            string driveId = file.GoogleDriveFileId;
            string fileName = file.FileName;
            string mimeType = file.MimeType ?? "application/octet-stream";

            if (versionId.HasValue)
            {
                var version = await _fileRepo.GetFileVersionByIdAsync(versionId.Value);
                if (version != null && version.ProjectFileId == fileId)
                {
                    driveId = version.GoogleDriveFileId;
                    fileName = version.FileName;
                    mimeType = version.MimeType ?? mimeType;
                }
            }

            try
            {
                var result = await _googleDriveService.DownloadFileAsync(driveId);
                return (result.ContentStream, !string.IsNullOrWhiteSpace(mimeType) ? mimeType : result.MimeType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download file from Google Drive with ID: {DriveId}", driveId);
                throw ApiException.InternalServerError($"Không thể tải tệp từ Google Drive: {ex.Message}");
            }
        }

        public async Task<(Stream Stream, string MimeType, string FileName)> DownloadMultipleFilesAsync(Guid projectId, Guid currentUserId, List<Guid> fileIds, List<Guid>? folderIds = null)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null)
            {
                throw ApiException.Forbidden("Bạn không có quyền truy cập tài liệu trong dự án này.");
            }

            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null)
            {
                throw ApiException.NotFound("Dự án không tồn tại.");
            }

            fileIds ??= new List<Guid>();
            folderIds ??= new List<Guid>();

            // If only 1 file and 0 folders selected, download directly as single file
            if (fileIds.Count == 1 && folderIds.Count == 0)
            {
                return await DownloadFileAsync(projectId, fileIds[0], currentUserId);
            }

            // Gather all files to include in zip
            var filesToZip = new List<(ProjectFile File, string RelativePath)>();
            var usedEntryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Direct files
            foreach (var fileId in fileIds.Distinct())
            {
                var file = await _fileRepo.GetFileByIdAsync(fileId);
                if (file != null && file.ProjectId == projectId && !file.IsDeleted)
                {
                    string entryName = GetUniqueEntryName(file.FileName, usedEntryNames);
                    filesToZip.Add((file, entryName));
                }
            }

            // 2. Folder files
            foreach (var folderId in folderIds.Distinct())
            {
                var folder = await _fileRepo.GetFolderByIdAsync(folderId);
                if (folder != null && folder.ProjectId == projectId && !folder.IsDeleted)
                {
                    var folderFiles = await _fileRepo.GetAllFilesInFolderHierarchyAsync(projectId, folderId);
                    foreach (var file in folderFiles)
                    {
                        string path = folder.Name + "/" + file.FileName;
                        string entryName = GetUniqueEntryName(path, usedEntryNames);
                        filesToZip.Add((file, entryName));
                    }
                }
            }

            if (filesToZip.Count == 0)
            {
                throw ApiException.BadRequest("Không tìm thấy tệp hợp lệ nào để tải về.");
            }

            // Create in-memory zip archive
            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var item in filesToZip)
                {
                    try
                    {
                        var driveResult = await _googleDriveService.DownloadFileAsync(item.File.GoogleDriveFileId);
                        var entry = archive.CreateEntry(item.RelativePath, CompressionLevel.Fastest);
                        using var entryStream = entry.Open();
                        await driveResult.ContentStream.CopyToAsync(entryStream);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to download file '{FileName}' ({FileId}) for zip batch", item.File.FileName, item.File.Id);
                    }
                }
            }

            memoryStream.Position = 0;
            string zipName = $"{project.Name}_files_{DateTime.Now:yyyyMMdd_HHmm}.zip";
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                zipName = zipName.Replace(c, '_');
            }

            return (memoryStream, "application/zip", zipName);
        }

        public async Task DeleteMultipleAsync(Guid projectId, Guid currentUserId, List<Guid> fileIds, List<Guid>? folderIds = null)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null)
            {
                throw ApiException.Forbidden("Bạn không có quyền thao tác trên dự án này.");
            }

            var user = await _userRepo.GetByIdAsync(currentUserId);
            bool isPrivileged = (user != null && string.Equals(user.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase)) || ProjectRoles.IsOwnerOrManager(member.Role);
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

        private static string GetUniqueEntryName(string desiredName, HashSet<string> usedNames)
        {
            if (usedNames.Add(desiredName)) return desiredName;

            var dir = Path.GetDirectoryName(desiredName)?.Replace('\\', '/');
            var nameWithoutExt = Path.GetFileNameWithoutExtension(desiredName);
            var ext = Path.GetExtension(desiredName);

            int counter = 2;
            while (true)
            {
                var candidate = string.IsNullOrEmpty(dir) 
                    ? $"{nameWithoutExt} ({counter}){ext}" 
                    : $"{dir}/{nameWithoutExt} ({counter}){ext}";

                if (usedNames.Add(candidate)) return candidate;
                counter++;
            }
        }

        public async Task<ProjectFileResponse> RenameFileAsync(Guid projectId, Guid fileId, Guid currentUserId, string newFileName)
        {
            if (string.IsNullOrWhiteSpace(newFileName))
            {
                throw ApiException.BadRequest("Tên tệp không được để trống.");
            }

            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null)
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
            if (member == null)
            {
                throw ApiException.Forbidden("Bạn không có quyền truy cập vào dự án này.");
            }

            if (!ProjectRoles.IsOwnerOrManager(member.Role))
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

        // ==================== FOLDERS ====================

        public async Task<ProjectFolderResponse> CreateFolderAsync(Guid projectId, Guid currentUserId, string name, Guid? parentFolderId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw ApiException.BadRequest("Tên thư mục không được để trống.");
            }

            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null)
            {
                throw ApiException.Forbidden("Bạn không có quyền thao tác trên dự án này.");
            }

            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null)
            {
                throw ApiException.NotFound("Dự án không tồn tại.");
            }

            var projectDriveFolderId = await EnsureProjectDriveFolderAsync(project);

            string parentDriveFolderId = projectDriveFolderId;
            if (parentFolderId.HasValue)
            {
                var parentFolder = await _fileRepo.GetFolderByIdAsync(parentFolderId.Value);
                if (parentFolder == null || parentFolder.ProjectId != projectId)
                {
                    throw ApiException.NotFound("Thư mục cha không tồn tại.");
                }

                if (!string.IsNullOrWhiteSpace(parentFolder.GoogleDriveFolderId))
                {
                    parentDriveFolderId = parentFolder.GoogleDriveFolderId;
                }
            }

            string? driveFolderId = null;
            try
            {
                driveFolderId = await _googleDriveService.CreateFolderAsync(name.Trim(), parentDriveFolderId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not create folder '{Name}' on Google Drive. Proceeding with DB folder.", name);
            }

            var user = await _userRepo.GetByIdAsync(currentUserId);
            var folder = new ProjectFolder
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                ParentFolderId = parentFolderId,
                Name = name.Trim(),
                GoogleDriveFolderId = driveFolderId,
                CreatedById = currentUserId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var savedFolder = await _fileRepo.AddFolderAsync(folder);

            await _fileRepo.AddActivityAsync(new ProjectFileActivity
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = currentUserId,
                ActionType = "CreateFolder",
                TargetName = folder.Name,
                Details = parentFolderId.HasValue ? "Tạo thư mục con" : "Tạo thư mục gốc",
                CreatedAt = DateTime.UtcNow
            });

            return MapToFolderResponse(savedFolder, user?.FullName ?? user?.Email);
        }

        public async Task<ProjectFolderResponse> RenameFolderAsync(Guid projectId, Guid folderId, Guid currentUserId, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw ApiException.BadRequest("Tên thư mục không được để trống.");
            }

            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null || !ProjectRoles.IsOwnerOrManager(member.Role))
            {
                throw ApiException.Forbidden("Chỉ Quản lý hoặc Chủ sở hữu mới có quyền đổi tên thư mục.");
            }

            var folder = await _fileRepo.GetFolderByIdAsync(folderId);
            if (folder == null || folder.ProjectId != projectId)
            {
                throw ApiException.NotFound("Thư mục không tồn tại.");
            }

            var oldName = folder.Name;
            folder.Name = newName.Trim();
            await _fileRepo.UpdateFolderAsync(folder);

            await _fileRepo.AddActivityAsync(new ProjectFileActivity
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = currentUserId,
                ActionType = "RenameFolder",
                TargetName = folder.Name,
                Details = $"Đổi tên thư mục từ '{oldName}' thành '{folder.Name}'",
                CreatedAt = DateTime.UtcNow
            });

            return MapToFolderResponse(folder);
        }

        public async Task DeleteFolderAsync(Guid projectId, Guid folderId, Guid currentUserId)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null || !ProjectRoles.IsOwnerOrManager(member.Role))
            {
                throw ApiException.Forbidden("Chỉ Quản lý hoặc Chủ sở hữu mới có quyền xóa thư mục.");
            }

            var folder = await _fileRepo.GetFolderByIdAsync(folderId);
            if (folder == null || folder.ProjectId != projectId)
            {
                throw ApiException.NotFound("Thư mục không tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(folder.GoogleDriveFolderId))
            {
                try
                {
                    await _googleDriveService.DeleteFileAsync(folder.GoogleDriveFolderId, permanent: false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not trash folder {DriveId} on Google Drive", folder.GoogleDriveFolderId);
                }
            }

            await _fileRepo.DeleteFolderAsync(folder);

            await _fileRepo.AddActivityAsync(new ProjectFileActivity
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = currentUserId,
                ActionType = "DeleteFolder",
                TargetName = folder.Name,
                Details = "Đã xóa thư mục khỏi dự án",
                CreatedAt = DateTime.UtcNow
            });
        }

        // ==================== ACTIVITIES ====================

        public async Task<List<ProjectFileActivityResponse>> GetActivitiesAsync(Guid projectId, Guid currentUserId)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member == null)
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

        private static ProjectFolderResponse MapToFolderResponse(ProjectFolder f, string? createdByName = null)
        {
            return new ProjectFolderResponse
            {
                Id = f.Id,
                ProjectId = f.ProjectId,
                ParentFolderId = f.ParentFolderId,
                Name = f.Name,
                GoogleDriveFolderId = f.GoogleDriveFolderId,
                CreatedById = f.CreatedById,
                CreatedByName = createdByName ?? (f.CreatedBy != null && !string.IsNullOrWhiteSpace(f.CreatedBy.FullName) ? f.CreatedBy.FullName : (f.CreatedBy?.Email ?? "Thành viên")),
                CreatedAt = f.CreatedAt,
                FileCount = f.Files?.Count(file => !file.IsDeleted) ?? 0,
                SubFolderCount = f.SubFolders?.Count(sub => !sub.IsDeleted) ?? 0
            };
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
