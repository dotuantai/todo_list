using API_v2.Models;

namespace API_v2.Repositorys.IRepositorys
{
    public interface ITaskAssignmentRepository
    {
        bool Exists(int taskId, Guid userId);
        TaskAssignment? GetAssignment(int taskId, Guid userId);
        List<TaskAssignment> GetAssignedTasks(Guid userId);
        void Add(TaskAssignment assignment);
        void Remove(TaskAssignment assignment);
        void Save();
    }
}
