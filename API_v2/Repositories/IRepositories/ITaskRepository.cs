using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models;
using API_v2.Models.DTOs;

namespace API_v2.Repositories.IRepositories
{
    public interface ITaskRepository
    {
        Task<TodoTask?> GetByIdAsync(int id);
        Task<TodoTask?> GetByIdWithDetailsAsync(int id);
        Task<(List<TodoTask> items, int totalCount)> GetTasksByProjectIdAsync(Guid projectId, int? columnId, int page, int pageSize);
        Task<TaskStatsResponse> GetTaskStatsByProjectIdAsync(Guid projectId);
        void Add(TodoTask task);
        void Delete(TodoTask task);
        Task SaveAsync();
    }
}
