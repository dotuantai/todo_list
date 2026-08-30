namespace API_v2.Repositories.IRepositories;

public sealed record TaskFeedRecord(
    string Type, int Id, DateTime CreatedAt, Guid UserId,
    string UserName, string? Content, string? ChangesJson);

public interface ITaskFeedRepository
{
    Task<(List<TaskFeedRecord> Items, int TotalCount)> GetTaskFeedAsync(int taskId, int page, int pageSize);
}
