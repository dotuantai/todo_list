using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_v2.Controllers
{
    [Route("api/projects/{projectId:guid}/files")]
    [Authorize]
    public class ProjectFileController : BaseApiController
    {
        private readonly IProjectFileService _fileService;
        private readonly IProjectFolderService _folderService;
        private readonly IProjectFileTransferService _transferService;

        public ProjectFileController(
            IProjectFileService fileService,
            IProjectFolderService folderService,
            IProjectFileTransferService transferService)
        {
            _fileService = fileService;
            _folderService = folderService;
            _transferService = transferService;
        }

        // ==================== EXPLORER & FILES ====================

        [HttpGet("explorer")]
        public async Task<ActionResult> GetExplorer(Guid projectId, [FromQuery] Guid? folderId = null, [FromQuery] int? taskId = null)
        {
            var result = await _folderService.GetExplorerAsync(projectId, CurrentUserId, folderId, taskId);
            return Ok(new ApiResponse<ProjectFilesExplorerResponse>(true, "Lấy dữ liệu tệp và thư mục thành công.", result));
        }

        [HttpGet]
        public async Task<ActionResult> GetFiles(Guid projectId, [FromQuery] Guid? folderId = null, [FromQuery] int? taskId = null)
        {
            var files = await _fileService.GetFilesAsync(projectId, CurrentUserId, folderId, taskId);
            return Ok(new ApiResponse<List<ProjectFileResponse>>(true, "Lấy danh sách tài liệu thành công.", files));
        }

        [HttpPost]
        [RequestSizeLimit(52428800)] // 50 MB
        public async Task<ActionResult> UploadFile(Guid projectId, IFormFile file, [FromForm] Guid? folderId = null, [FromForm] int? taskId = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<object>(false, "Vui lòng chọn tệp để tải lên.", null));
            }

            await using var stream = file.OpenReadStream();
            var result = await _fileService.UploadFileAsync(
                projectId, CurrentUserId, stream, file.FileName, file.ContentType, file.Length, folderId, taskId);
            return Ok(new ApiResponse<ProjectFileResponse>(true, "Tải tệp lên thành công.", result));
        }

        [HttpPost("{fileId:guid}/version")]
        [RequestSizeLimit(52428800)] // 50 MB
        public async Task<ActionResult> UpdateFileVersion(Guid projectId, Guid fileId, IFormFile file, [FromForm] string? changeNote = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<object>(false, "Vui lòng chọn tệp mới để cập nhật phiên bản.", null));
            }

            await using var stream = file.OpenReadStream();
            var result = await _transferService.UpdateFileVersionAsync(
                projectId, fileId, CurrentUserId, stream, file.FileName, file.ContentType, file.Length, changeNote);
            return Ok(new ApiResponse<ProjectFileResponse>(true, "Cập nhật phiên bản tệp thành công.", result));
        }

        [HttpGet("{fileId:guid}/history")]
        public async Task<ActionResult> GetFileVersions(Guid projectId, Guid fileId)
        {
            var versions = await _transferService.GetFileVersionsAsync(projectId, fileId, CurrentUserId);
            return Ok(new ApiResponse<List<ProjectFileVersionResponse>>(true, "Lấy lịch sử phiên bản tệp thành công.", versions));
        }

        [HttpGet("{fileId:guid}/download")]
        public async Task<IActionResult> DownloadFile(Guid projectId, Guid fileId, [FromQuery] Guid? versionId = null)
        {
            var (stream, mimeType, fileName) = await _transferService.DownloadFileAsync(projectId, fileId, CurrentUserId, versionId);
            return File(stream, mimeType, fileName, enableRangeProcessing: true);
        }

        [HttpPut("{fileId:guid}/rename")]
        public async Task<ActionResult> RenameFile(Guid projectId, Guid fileId, [FromBody] RenameProjectFileRequest request)
        {
            var result = await _fileService.RenameFileAsync(projectId, fileId, CurrentUserId, request.FileName);
            return Ok(new ApiResponse<ProjectFileResponse>(true, "Đổi tên tệp thành công.", result));
        }

        [HttpDelete("{fileId:guid}")]
        public async Task<ActionResult> DeleteFile(Guid projectId, Guid fileId)
        {
            await _fileService.DeleteFileAsync(projectId, fileId, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Xóa tài liệu thành công.", null));
        }

        [HttpPost("batch-download")]
        public async Task<IActionResult> BatchDownload(Guid projectId, [FromBody] BatchDownloadRequestDTO request)
        {
            var (stream, mimeType, fileName) = await _transferService.DownloadMultipleFilesAsync(projectId, CurrentUserId, request.FileIds, request.FolderIds);
            return File(stream, mimeType, fileName);
        }

        [HttpPost("batch-delete")]
        public async Task<ActionResult> BatchDelete(Guid projectId, [FromBody] BatchDeleteRequestDTO request)
        {
            await _fileService.DeleteMultipleAsync(projectId, CurrentUserId, request.FileIds, request.FolderIds);
            return Ok(new ApiResponse<object>(true, "Xóa hàng loạt thành công.", null));
        }

        // ==================== FOLDERS ====================

        [HttpPost("folders")]
        public async Task<ActionResult> CreateFolder(Guid projectId, [FromBody] CreateProjectFolderRequest request)
        {
            var result = await _folderService.CreateFolderAsync(projectId, CurrentUserId, request.Name, request.ParentFolderId);
            return Ok(new ApiResponse<ProjectFolderResponse>(true, "Tạo thư mục thành công.", result));
        }

        [HttpPut("folders/{folderId:guid}/rename")]
        public async Task<ActionResult> RenameFolder(Guid projectId, Guid folderId, [FromBody] RenameProjectFolderRequest request)
        {
            var result = await _folderService.RenameFolderAsync(projectId, folderId, CurrentUserId, request.Name);
            return Ok(new ApiResponse<ProjectFolderResponse>(true, "Đổi tên thư mục thành công.", result));
        }

        [HttpDelete("folders/{folderId:guid}")]
        public async Task<ActionResult> DeleteFolder(Guid projectId, Guid folderId)
        {
            await _folderService.DeleteFolderAsync(projectId, folderId, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Xóa thư mục thành công.", null));
        }

        // ==================== ACTIVITIES ====================

        [HttpGet("activities")]
        public async Task<ActionResult> GetActivities(Guid projectId)
        {
            var activities = await _fileService.GetActivitiesAsync(projectId, CurrentUserId);
            return Ok(new ApiResponse<List<ProjectFileActivityResponse>>(true, "Lấy lịch sử hoạt động thành công.", activities));
        }
    }
}
