using System.IO.Compression;
using API_v2.Exceptions;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace API_v2.Services
{
    public class ProjectFileTransferService : IProjectFileTransferService
    {
        private readonly IProjectFileRepository _fileRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGoogleDriveService _googleDriveService;
        private readonly ILogger<ProjectFileTransferService> _logger;

        public ProjectFileTransferService(
            IProjectFileRepository fileRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IGoogleDriveService googleDriveService,
            ILogger<ProjectFileTransferService> logger)
        {
            _fileRepository = fileRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _googleDriveService = googleDriveService;
            _logger = logger;
        }

        public async Task<ProjectFileResponse> UpdateFileVersionAsync(
            Guid projectId,
            Guid fileId,
            Guid currentUserId,
            IFormFile file,
            string? changeNote = null)
        {
            if (file.Length == 0) throw ApiException.BadRequest("A valid file is required.");
            await EnsureMemberAsync(projectId, currentUserId);
            var projectFile = await GetProjectFileAsync(projectId, fileId);
            var project = await _projectRepository.GetByIdAsync(projectId)
                ?? throw ApiException.NotFound("Project not found.");
            var targetDriveFolderId = await EnsureProjectDriveFolderAsync(project);
            if (!string.IsNullOrWhiteSpace(projectFile.Folder?.GoogleDriveFolderId))
            {
                targetDriveFolderId = projectFile.Folder.GoogleDriveFolderId;
            }

            string driveFileId;
            try
            {
                await using var stream = file.OpenReadStream();
                driveFileId = (await _googleDriveService.UploadFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    targetDriveFolderId)).FileId;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to upload a new version for file {FileId}", fileId);
                throw ApiException.InternalServerError("Could not upload the new file version.");
            }

            var versionNumber = projectFile.CurrentVersion + 1;
            await _fileRepository.AddFileVersionAsync(new ProjectFileVersion
            {
                Id = Guid.NewGuid(),
                ProjectFileId = projectFile.Id,
                VersionNumber = versionNumber,
                GoogleDriveFileId = driveFileId,
                FileName = file.FileName,
                FileSize = file.Length,
                MimeType = file.ContentType,
                ChangeNote = string.IsNullOrWhiteSpace(changeNote) ? $"Version {versionNumber}" : changeNote,
                UploadedById = currentUserId,
                CreatedAt = DateTime.UtcNow
            });

            projectFile.GoogleDriveFileId = driveFileId;
            projectFile.FileName = file.FileName;
            projectFile.FileSize = file.Length;
            projectFile.MimeType = file.ContentType;
            projectFile.CurrentVersion = versionNumber;
            projectFile.UpdatedAt = DateTime.UtcNow;
            projectFile.UpdatedById = currentUserId;
            await _fileRepository.UpdateFileAsync(projectFile);
            await AddActivityAsync(projectId, currentUserId, file.FileName, versionNumber, changeNote);

            var user = await _userRepository.GetByIdAsync(currentUserId);
            return MapFile(projectFile, user?.FullName ?? user?.Email);
        }

        public async Task<List<ProjectFileVersionResponse>> GetFileVersionsAsync(
            Guid projectId,
            Guid fileId,
            Guid currentUserId)
        {
            await EnsureMemberAsync(projectId, currentUserId);
            await GetProjectFileAsync(projectId, fileId);
            return (await _fileRepository.GetFileVersionsAsync(fileId)).Select(version => new ProjectFileVersionResponse
            {
                Id = version.Id,
                ProjectFileId = version.ProjectFileId,
                VersionNumber = version.VersionNumber,
                GoogleDriveFileId = version.GoogleDriveFileId,
                FileName = version.FileName,
                FileSize = version.FileSize,
                MimeType = version.MimeType,
                ChangeNote = version.ChangeNote,
                UploadedById = version.UploadedById,
                UploadedByName = version.UploadedBy?.FullName ?? version.UploadedBy?.Email ?? "Member",
                UploadedByEmail = version.UploadedBy?.Email ?? string.Empty,
                CreatedAt = version.CreatedAt
            }).ToList();
        }

        public async Task<(Stream Stream, string MimeType, string FileName)> DownloadFileAsync(
            Guid projectId,
            Guid fileId,
            Guid currentUserId,
            Guid? versionId = null)
        {
            await EnsureMemberAsync(projectId, currentUserId);
            var file = await GetProjectFileAsync(projectId, fileId);
            var driveId = file.GoogleDriveFileId;
            var fileName = file.FileName;
            var mimeType = file.MimeType ?? "application/octet-stream";

            if (versionId.HasValue)
            {
                var version = await _fileRepository.GetFileVersionByIdAsync(versionId.Value);
                if (version is null || version.ProjectFileId != fileId)
                {
                    throw ApiException.NotFound("File version not found.");
                }

                driveId = version.GoogleDriveFileId;
                fileName = version.FileName;
                mimeType = version.MimeType ?? mimeType;
            }

            try
            {
                var result = await _googleDriveService.DownloadFileAsync(driveId);
                return (result.ContentStream, mimeType, fileName);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to download Google Drive file {DriveId}", driveId);
                throw ApiException.InternalServerError("Could not download the file.");
            }
        }

        public async Task<(Stream Stream, string MimeType, string FileName)> DownloadMultipleFilesAsync(
            Guid projectId,
            Guid currentUserId,
            List<Guid> fileIds,
            List<Guid>? folderIds = null)
        {
            await EnsureMemberAsync(projectId, currentUserId);
            var project = await _projectRepository.GetByIdAsync(projectId)
                ?? throw ApiException.NotFound("Project not found.");
            folderIds ??= [];
            if (fileIds.Count == 1 && folderIds.Count == 0)
            {
                return await DownloadFileAsync(projectId, fileIds[0], currentUserId);
            }

            var files = await CollectFilesAsync(projectId, fileIds, folderIds);
            if (files.Count == 0) throw ApiException.BadRequest("No valid files were selected.");

            var temporaryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read,
                64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);
            try
            {
                using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var (file, entryName) in files)
                    {
                        try
                        {
                            var driveFile = await _googleDriveService.DownloadFileAsync(file.GoogleDriveFileId);
                            await using (driveFile.ContentStream)
                            await using (var entryStream = archive.CreateEntry(entryName, CompressionLevel.Fastest).Open())
                            {
                                await driveFile.ContentStream.CopyToAsync(entryStream);
                            }
                        }
                        catch (Exception exception)
                        {
                            _logger.LogWarning(exception, "Could not add file {FileId} to archive", file.Id);
                        }
                    }
                }

                output.Position = 0;
                var zipName = SanitizeFileName($"{project.Name}_files_{DateTime.UtcNow:yyyyMMdd_HHmm}.zip");
                return (output, "application/zip", zipName);
            }
            catch
            {
                await output.DisposeAsync();
                throw;
            }
        }

        private async Task<List<(ProjectFile File, string EntryName)>> CollectFilesAsync(
            Guid projectId,
            IEnumerable<Guid> fileIds,
            IEnumerable<Guid> folderIds)
        {
            var result = new List<(ProjectFile, string)>();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var fileId in fileIds.Distinct())
            {
                var file = await _fileRepository.GetFileByIdAsync(fileId);
                if (file is not null && file.ProjectId == projectId && !file.IsDeleted)
                    result.Add((file, GetUniqueEntryName(file.FileName, names)));
            }

            foreach (var folderId in folderIds.Distinct())
            {
                var folder = await _fileRepository.GetFolderByIdAsync(folderId);
                if (folder is null || folder.ProjectId != projectId || folder.IsDeleted) continue;
                foreach (var file in await _fileRepository.GetAllFilesInFolderHierarchyAsync(projectId, folderId))
                    result.Add((file, GetUniqueEntryName($"{folder.Name}/{file.FileName}", names)));
            }
            return result;
        }

        private async Task EnsureMemberAsync(Guid projectId, Guid userId)
        {
            if (await _projectRepository.GetMemberAsync(projectId, userId) is null)
                throw ApiException.Forbidden("You do not have access to this project's files.");
        }

        private async Task<ProjectFile> GetProjectFileAsync(Guid projectId, Guid fileId)
        {
            var file = await _fileRepository.GetFileByIdAsync(fileId);
            return file is not null && file.ProjectId == projectId
                ? file
                : throw ApiException.NotFound("File not found.");
        }

        private async Task<string> EnsureProjectDriveFolderAsync(Project project)
        {
            if (!string.IsNullOrWhiteSpace(project.GoogleDriveFolderId)) return project.GoogleDriveFolderId;
            project.GoogleDriveFolderId = await _googleDriveService.CreateFolderAsync($"{project.Name}_{project.Id}");
            await _projectRepository.SaveAsync();
            return project.GoogleDriveFolderId;
        }

        private Task AddActivityAsync(Guid projectId, Guid userId, string name, int version, string? note)
            => _fileRepository.AddActivityAsync(new ProjectFileActivity
            {
                Id = Guid.NewGuid(), ProjectId = projectId, UserId = userId,
                ActionType = "UpdateVersion", TargetName = name,
                Details = $"Updated to version {version}" + (string.IsNullOrWhiteSpace(note) ? string.Empty : $": {note}"),
                CreatedAt = DateTime.UtcNow
            });

        private static string GetUniqueEntryName(string desiredName, HashSet<string> usedNames)
        {
            if (usedNames.Add(desiredName)) return desiredName;
            var directory = Path.GetDirectoryName(desiredName)?.Replace('\\', '/');
            var baseName = Path.GetFileNameWithoutExtension(desiredName);
            var extension = Path.GetExtension(desiredName);
            for (var counter = 2; ; counter++)
            {
                var candidate = string.IsNullOrEmpty(directory)
                    ? $"{baseName} ({counter}){extension}"
                    : $"{directory}/{baseName} ({counter}){extension}";
                if (usedNames.Add(candidate)) return candidate;
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            foreach (var character in Path.GetInvalidFileNameChars()) fileName = fileName.Replace(character, '_');
            return fileName;
        }

        private static ProjectFileResponse MapFile(ProjectFile file, string? updatedByName) => new()
        {
            Id = file.Id, ProjectId = file.ProjectId, FolderId = file.FolderId,
            FolderName = file.Folder?.Name, TaskId = file.TaskId, TaskTitle = file.Task?.Title,
            GoogleDriveFileId = file.GoogleDriveFileId, FileName = file.FileName,
            FileSize = file.FileSize, MimeType = file.MimeType, CurrentVersion = file.CurrentVersion,
            UploadedById = file.UploadedById,
            UploadedByName = file.UploadedBy?.FullName ?? file.UploadedBy?.Email ?? "Member",
            UploadedByEmail = file.UploadedBy?.Email ?? string.Empty,
            CreatedAt = file.CreatedAt, UpdatedAt = file.UpdatedAt, UpdatedByName = updatedByName,
            VersionCount = file.Versions?.Count > 0 ? file.Versions.Count : Math.Max(file.CurrentVersion, 1)
        };
    }
}
