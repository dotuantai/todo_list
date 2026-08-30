using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories;

public class TaskCommentRepository : ITaskCommentRepository
{
    private readonly AppDbContext _db;

    public TaskCommentRepository(AppDbContext db) => _db = db;

    public async Task<(List<TaskComment> Items, int TotalCount)> GetByTaskIdAsync(int taskId, int page, int pageSize)
    {
        var query = _db.TaskComments.AsNoTracking().Include(comment => comment.User)
            .Where(comment => comment.TaskId == taskId);
        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(comment => comment.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public Task<TaskComment?> GetByIdAsync(int commentId) => _db.TaskComments
        .Include(comment => comment.User).FirstOrDefaultAsync(comment => comment.Id == commentId);

    public void Add(TaskComment comment) => _db.TaskComments.Add(comment);
    public void Remove(TaskComment comment) => _db.TaskComments.Remove(comment);
    public Task SaveAsync() => _db.SaveChangesAsync();
}
