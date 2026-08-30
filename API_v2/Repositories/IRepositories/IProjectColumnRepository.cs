using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models;

namespace API_v2.Repositories.IRepositories
{
    public interface IProjectColumnRepository
    {
        Task<List<ProjectColumn>> GetColumnsByProjectIdAsync(Guid projectId);
        Task<ProjectColumn?> GetByIdAsync(int id);
        Task<(ProjectColumn Column, List<ProjectColumn> Columns)> CreateAtOrderAsync(ProjectColumn column, int requestedOrder);
        Task<(ProjectColumn Column, List<ProjectColumn> Columns)> UpdateAndReorderAsync(int columnId, string name, int requestedOrder, bool isCompletedStage);
        void Add(ProjectColumn column);
        void Delete(ProjectColumn column);
        Task SaveAsync();
    }
}
