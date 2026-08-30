namespace API_v2.Services.Interfaces;

public interface ITaskImportExportService
{
    Task<byte[]> GetTaskTemplateAsync();
    Task<int> ImportTasksAsync(Guid projectId, Guid currentUserId, Stream fileStream, string fileName);
}
