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
        Task<List<TodoTask>> GetTasksByProjectIdAsync(Guid projectId);
        void Add(TodoTask task);
        void Delete(TodoTask task);
        Task SaveAsync();
    }
}
