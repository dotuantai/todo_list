using API_v2.Datas;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _db;
    public AdminRepository(AppDbContext db) => _db = db;

    public async Task<AdminTotals> GetTotalsAsync() => new(
        await _db.Users.CountAsync(), await _db.Projects.CountAsync(), await _db.Tasks.CountAsync());

    public async Task<TaskStatusCounts> GetTaskStatusCountsAsync()
    {
        return await _db.Tasks.GroupBy(_ => 1).Select(group => new TaskStatusCounts(
            group.Count(task => !task.Column.IsCompletedStage && task.Column.Order == 0),
            group.Count(task => !task.Column.IsCompletedStage && task.Column.Order != 0),
            group.Count(task => task.Column.IsCompletedStage))).FirstOrDefaultAsync()
            ?? new TaskStatusCounts(0, 0, 0);
    }

    public async Task<(List<DailyRegistrationCount> Projects, List<DailyRegistrationCount> Users)> GetRegistrationsAsync(DateTime since)
    {
        var projects = await _db.Projects.Where(project => project.CreatedAt >= since)
            .GroupBy(project => project.CreatedAt.Date)
            .Select(group => new DailyRegistrationCount(group.Key, group.Count())).ToListAsync();
        var users = await _db.Users.Where(user => user.CreatedAt >= since)
            .GroupBy(user => user.CreatedAt.Date)
            .Select(group => new DailyRegistrationCount(group.Key, group.Count())).ToListAsync();
        return (projects, users);
    }

    public async Task<List<ProjectHealthSummary>> GetTopProjectHealthAsync(DateTime now, int limit)
    {
        return await _db.Projects.AsNoTracking().OrderByDescending(project => project.Tasks.Count).Take(limit)
            .Select(project => new ProjectHealthSummary(
                project.Name,
                project.Tasks.Count(task => !task.Column.IsCompletedStage && task.Deadline >= now && task.Column.Order == 0),
                project.Tasks.Count(task => !task.Column.IsCompletedStage && task.Deadline >= now && task.Column.Order != 0),
                project.Tasks.Count(task => task.Column.IsCompletedStage),
                project.Tasks.Count(task => !task.Column.IsCompletedStage && task.Deadline < now)))
            .ToListAsync();
    }
}
