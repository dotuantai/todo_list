using API_v2.Datas;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories;

public class TaskFeedRepository : ITaskFeedRepository
{
    private readonly AppDbContext _db;
    public TaskFeedRepository(AppDbContext db) => _db = db;

    public async Task<(List<TaskFeedRecord> Items, int TotalCount)> GetTaskFeedAsync(int taskId, int page, int pageSize)
    {
        var comments = _db.TaskComments.AsNoTracking().Where(comment => comment.TaskId == taskId)
            .Select(comment => new TaskFeedRecord("comment", comment.Id, comment.CreatedAt,
                comment.UserId, comment.User.Email, comment.Content, null));
        var activities = _db.TaskActivities.AsNoTracking().Where(activity => activity.TaskId == taskId)
            .Select(activity => new TaskFeedRecord("activity", activity.Id, activity.ChangedAt,
                activity.UserId, activity.User.Email, null, activity.Changes));
        var combined = comments.Concat(activities);
        var totalCount = await combined.CountAsync();
        var items = await combined.OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }
}
