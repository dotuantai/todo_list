using System.IO;
using System.Threading.Tasks;

namespace API_v2.Services.Interfaces
{
    public interface IGoogleDriveService
    {
        Task<string> CreateFolderAsync(string folderName, string? parentFolderId = null);
        Task<(string FileId, string? WebViewLink)> UploadFileAsync(Stream fileStream, string fileName, string mimeType, string folderId);
        Task<(Stream ContentStream, string MimeType, string FileName)> DownloadFileAsync(string fileId);
        Task<bool> DeleteFileAsync(string fileId, bool permanent = false);
    }
}
