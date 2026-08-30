using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories;

public class TaskActivityRepository : ITaskActivityRepository
{
    private readonly AppDbContext _db;
    public TaskActivityRepository(AppDbContext db) => _db = db;

    public Task<List<TaskActivity>> GetByTaskIdAsync(int taskId) => _db.TaskActivities
        .AsNoTracking().Include(activity => activity.User)
        .Where(activity => activity.TaskId == taskId)
        .OrderBy(activity => activity.ChangedAt).ToListAsync();

    public void Add(TaskActivity activity) => _db.TaskActivities.Add(activity);
}
