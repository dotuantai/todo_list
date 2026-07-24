using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories
{
    public class TaskAssignmentRepository : ITaskAssignmentRepository
    {
        private readonly AppDbContext _db;

        public TaskAssignmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ExistsAsync(int taskId, Guid userId)
        {
            return await _db.TaskAssignments.AnyAsync(x => x.TaskId == taskId && x.UserId == userId);
        }

        public async Task<TaskAssignment?> GetAssignmentAsync(int taskId, Guid userId)
        {
            return await _db.TaskAssignments.FirstOrDefaultAsync(x => x.TaskId == taskId && x.UserId == userId);
        }

        public async Task<List<TaskAssignment>> GetAssignedTasksAsync(Guid userId)
        {
            return await _db.TaskAssignments
                .Include(x => x.Task)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AssignedAt)
                .ToListAsync();
        }

        public void Add(TaskAssignment assignment)
        {
            _db.TaskAssignments.Add(assignment);
        }

        public void Remove(TaskAssignment assignment)
        {
            _db.TaskAssignments.Remove(assignment);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
