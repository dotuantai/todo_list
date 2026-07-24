using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models;

namespace API_v2.Repositorys.IRepositorys
{
    public interface ITaskAssignmentRepository
    {
        Task<bool> ExistsAsync(int taskId, Guid userId);
        Task<TaskAssignment?> GetAssignmentAsync(int taskId, Guid userId);
        Task<List<TaskAssignment>> GetAssignedTasksAsync(Guid userId);
        void Add(TaskAssignment assignment);
        void Remove(TaskAssignment assignment);
        Task SaveAsync();
    }
}
