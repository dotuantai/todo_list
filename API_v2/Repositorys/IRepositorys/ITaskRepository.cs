using API_v2.Models;

namespace API_v2.Repositorys.IRepositorys
{
    public interface ITaskRepository
    {
        TodoTask? GetById(int id);
        TodoTask? GetByIdWithDetails(int id);
        List<TodoTask> GetTasksByProjectId(Guid projectId);
        void Add(TodoTask task);
        void Delete(TodoTask task);
        void Save();
    }
}
