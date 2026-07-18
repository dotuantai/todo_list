using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositorys.IRepositorys;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositorys
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _db;

        public TaskRepository(AppDbContext db)
        {
            _db = db;
        }

        public TodoTask? GetById(int id)
        {
            return _db.Tasks.FirstOrDefault(x => x.Id == id);
        }

        public List<TodoTask> GetTasksByProjectId(Guid projectId)
        {
            return _db.Tasks
                .Include(x => x.Assignments)
                .ThenInclude(a => a.User)
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public void Add(TodoTask task)
        {
            _db.Tasks.Add(task);
        }

        public void Delete(TodoTask task)
        {
            _db.Tasks.Remove(task);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
