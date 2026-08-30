using API_v2.Models;

namespace API_v2.Repositories.IRepositories;

public interface ITaskActivityRepository
{
    Task<List<TaskActivity>> GetByTaskIdAsync(int taskId);
    void Add(TaskActivity activity);
}
