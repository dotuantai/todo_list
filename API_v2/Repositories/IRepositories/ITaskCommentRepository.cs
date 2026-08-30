using API_v2.Models;

namespace API_v2.Repositories.IRepositories;

public interface ITaskCommentRepository
{
    Task<(List<TaskComment> Items, int TotalCount)> GetByTaskIdAsync(int taskId, int page, int pageSize);
    Task<TaskComment?> GetByIdAsync(int commentId);
    void Add(TaskComment comment);
    void Remove(TaskComment comment);
    Task SaveAsync();
}
