using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositorys.IRepositorys;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositorys
{
    public class TaskAssignmentRepository : ITaskAssignmentRepository
    {
        private readonly AppDbContext _db;

        public TaskAssignmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public bool Exists(int taskId, Guid userId)
        {
            return _db.TaskAssignments.Any(x => x.TaskId == taskId && x.UserId == userId);
        }

        public TaskAssignment? GetAssignment(int taskId, Guid userId)
        {
            return _db.TaskAssignments.FirstOrDefault(x => x.TaskId == taskId && x.UserId == userId);
        }

        public List<TaskAssignment> GetAssignedTasks(Guid userId)
        {
            return _db.TaskAssignments
                .Include(x => x.Task)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AssignedAt)
                .ToList();
        }

        public void Add(TaskAssignment assignment)
        {
            _db.TaskAssignments.Add(assignment);
        }

        public void Remove(TaskAssignment assignment)
        {
            _db.TaskAssignments.Remove(assignment);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
