using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using API_v2.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace API_v2.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleDriveService> _logger;
        private DriveService? _driveService;
        private readonly object _lock = new();

        public GoogleDriveService(IConfiguration configuration, ILogger<GoogleDriveService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private DriveService GetDriveService()
        {
            if (_driveService != null) return _driveService;

            lock (_lock)
            {
                if (_driveService != null) return _driveService;

                var authType = _configuration["GoogleDrive:AuthType"] ?? "ServiceAccount";

                // --- CHẾ ĐỘ 1: XÁC THỰC BẰNG OAUTH2 REFRESH TOKEN (Khuyên dùng cho Gmail cá nhân) ---
                if (string.Equals(authType, "OAuth2", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var clientId = _configuration["GoogleDrive:ClientId"] ?? _configuration["Google:ClientId"];
                        var clientSecret = _configuration["GoogleDrive:ClientSecret"];
                        var refreshToken = _configuration["GoogleDrive:RefreshToken"];

                        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(refreshToken))
                        {
                            throw new InvalidOperationException("Cấu hình OAuth2 Google Drive chưa đầy đủ. Vui lòng kiểm tra ClientId, ClientSecret và RefreshToken trong appsettings.json.");
                        }

                        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                        {
                            ClientSecrets = new ClientSecrets
                            {
                                ClientId = clientId,
                                ClientSecret = clientSecret
                            },
                            Scopes = new[] { DriveService.ScopeConstants.Drive }
                        });

                        var token = new TokenResponse { RefreshToken = refreshToken };
                        var credential = new UserCredential(flow, "user", token);

                        _driveService = new DriveService(new BaseClientService.Initializer
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = "TutaFlow"
                        });

                        _logger.LogInformation("Google Drive Service successfully initialized via OAuth2 Refresh Token.");
                        return _driveService;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to initialize Google Drive Service via OAuth2.");
                        throw new InvalidOperationException($"Lỗi khởi tạo kết nối Google Drive qua OAuth2: {ex.Message}", ex);
                    }
                }

                // --- CHẾ ĐỘ 2: XÁC THỰC BẰNG SERVICE ACCOUNT (Google Workspace / Shared Drive) ---
                var credentialsPath = _configuration["GoogleDrive:CredentialsPath"] ?? "Secrets/google-credentials.json";
                
                // Allow relative or absolute path
                if (!Path.IsPathRooted(credentialsPath))
                {
                    credentialsPath = Path.Combine(AppContext.BaseDirectory, credentialsPath);
                }

                if (!File.Exists(credentialsPath))
                {
                    _logger.LogWarning("Google Drive credentials file not found at: {Path}. Service is standing by.", credentialsPath);
                    throw new InvalidOperationException("Google Drive chưa được cấu hình. Vui lòng thêm tệp google-credentials.json vào thư mục cấu hình backend.");
                }

                try
                {
                    var jsonContent = File.ReadAllText(credentialsPath);
                    if (jsonContent.Contains("\"web\"") || jsonContent.Contains("\"installed\""))
                    {
                        throw new InvalidOperationException("Tệp JSON bạn vừa tải là 'OAuth 2.0 Client ID' (dùng cho Web Login), không phải 'Service Account Key'.");
                    }

                    GoogleCredential credential = GoogleCredential.FromJson(jsonContent)
                        .CreateScoped(DriveService.ScopeConstants.Drive);

                    _driveService = new DriveService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "TutaFlow"
                    });

                    _logger.LogInformation("Google Drive Service successfully initialized via Service Account.");
                    return _driveService;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Google Drive Service from credentials at {Path}", credentialsPath);
                    throw new InvalidOperationException($"Lỗi khởi tạo kết nối Google Drive: {ex.Message}", ex);
                }
            }
        }

        public async Task<string> CreateFolderAsync(string folderName, string? parentFolderId = null)
        {
            var service = GetDriveService();

            var defaultRootId = _configuration["GoogleDrive:RootFolderId"];
            var parent = !string.IsNullOrWhiteSpace(parentFolderId) ? parentFolderId : defaultRootId;

            var folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            if (!string.IsNullOrWhiteSpace(parent))
            {
                folderMetadata.Parents = new List<string> { parent };
            }
            else
            {
                folderMetadata.Parents = new List<string> { "root" };
            }

            try
            {
                var request = service.Files.Create(folderMetadata);
                request.Fields = "id, name, parents";
                request.SupportsAllDrives = true;

                var folder = await request.ExecuteAsync();
                _logger.LogInformation("Created Google Drive folder '{Name}' with ID: {Id} under Parent: {Parent}", folderName, folder.Id, parent);
                return folder.Id;
            }
            catch (Exception ex) when (!string.IsNullOrWhiteSpace(parent) && (ex.Message.Contains("NotFound", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("File not found", StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Parent folder {ParentId} not found. Creating folder at My Drive (root) instead...", parent);
                folderMetadata.Parents = new List<string> { "root" };
                var rootRequest = service.Files.Create(folderMetadata);
                rootRequest.Fields = "id, name, parents";
                rootRequest.SupportsAllDrives = true;

                var folder = await rootRequest.ExecuteAsync();
                _logger.LogInformation("Created Google Drive folder at root '{Name}' with ID: {Id}", folderName, folder.Id);
                return folder.Id;
            }
        }

        public async Task<(string FileId, string? WebViewLink)> UploadFileAsync(Stream fileStream, string fileName, string mimeType, string folderId)
        {
            var service = GetDriveService();

            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName
            };

            if (!string.IsNullOrWhiteSpace(folderId))
            {
                fileMetadata.Parents = new List<string> { folderId };
            }

            var request = service.Files.Create(fileMetadata, fileStream, string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType);
            request.Fields = "id, name, size, mimeType";
            request.SupportsAllDrives = true;

            var progress = await request.UploadAsync();
            if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
            {
                _logger.LogError(progress.Exception, "Google Drive upload failed for file '{FileName}'", fileName);
                throw new InvalidOperationException($"Tải file lên Google Drive thất bại: {progress.Exception?.Message}", progress.Exception);
            }

            var uploadedFile = request.ResponseBody;
            _logger.LogInformation(
                "File '{Name}' successfully uploaded privately to Google Drive with ID: {Id}",
                fileName,
                uploadedFile.Id);

            // Files remain private. Clients access content only through the
            // authorized project download endpoint.
            return (uploadedFile.Id, null);
        }

        public async Task<(Stream ContentStream, string MimeType, string FileName)> DownloadFileAsync(string fileId)
        {
            var service = GetDriveService();

            var getRequest = service.Files.Get(fileId);
            getRequest.Fields = "id, name, mimeType, size";
            getRequest.SupportsAllDrives = true;
            var fileMetadata = await getRequest.ExecuteAsync();

            // Google.Apis downloads into a caller-provided stream. Use a temporary,
            // seekable file instead of buffering the complete payload in managed RAM.
            // DeleteOnClose guarantees cleanup after ASP.NET disposes FileStreamResult.
            var temporaryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var outputStream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.Read,
                bufferSize: 64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);

            try
            {
                var downloadRequest = service.Files.Get(fileId);
                downloadRequest.SupportsAllDrives = true;
                await downloadRequest.DownloadAsync(outputStream);
                outputStream.Position = 0;

                return (outputStream, fileMetadata.MimeType ?? "application/octet-stream", fileMetadata.Name);
            }
            catch
            {
                await outputStream.DisposeAsync();
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileId, bool permanent = false)
        {
            var service = GetDriveService();

            try
            {
                if (permanent)
                {
                    var deleteRequest = service.Files.Delete(fileId);
                    deleteRequest.SupportsAllDrives = true;
                    await deleteRequest.ExecuteAsync();
                }
                else
                {
                    var updateFile = new Google.Apis.Drive.v3.Data.File { Trashed = true };
                    var updateRequest = service.Files.Update(updateFile, fileId);
                    updateRequest.SupportsAllDrives = true;
                    await updateRequest.ExecuteAsync();
                }
                _logger.LogInformation("Deleted file on Google Drive with ID: {Id} (Permanent={Permanent})", fileId, permanent);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file on Google Drive with ID: {Id}", fileId);
                return false;
            }
        }
    }
}
