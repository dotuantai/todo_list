using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models;

namespace API_v2.Repositories.IRepositories
{
    public interface ITaskRepository
    {
        Task<TodoTask?> GetByIdAsync(int id);
        Task<TodoTask?> GetByIdWithDetailsAsync(int id);
        Task<(List<TodoTask> items, int totalCount)> GetTasksByProjectIdAsync(Guid projectId, int? columnId, int page, int pageSize, string? search = null, API_v2.Models.Enums.TaskPriority? priority = null, Guid? assigneeId = null);
        Task<List<TaskColumnStatsRecord>> GetTaskStatsByProjectIdAsync(Guid projectId);
        void Add(TodoTask task);
        void Delete(TodoTask task);
        Task SaveAsync();
    }

    public sealed record TaskColumnStatsRecord(int ColumnId, string ColumnName, bool IsCompletedStage, int TaskCount);
}
