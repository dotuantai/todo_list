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

        public async Task<TodoTask?> GetByIdAsync(int id)
        {
            return await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<TodoTask?> GetByIdWithDetailsAsync(int id)
        {
            return await _db.Tasks
                .Include(x => x.Assignments)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<TodoTask>> GetTasksByProjectIdAsync(Guid projectId)
        {
            return await _db.Tasks
                .Include(x => x.Assignments)
                .ThenInclude(a => a.User)
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public void Add(TodoTask task)
        {
            _db.Tasks.Add(task);
        }

        public void Delete(TodoTask task)
        {
            _db.Tasks.Remove(task);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
