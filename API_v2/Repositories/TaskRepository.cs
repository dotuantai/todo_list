using API_v2.Datas;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories
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

        public async Task<(List<TodoTask> items, int totalCount)> GetTasksByProjectIdAsync(Guid projectId, int? columnId, int page, int pageSize, string search = null, API_v2.Models.Enums.TaskPriority? priority = null, Guid? assigneeId = null)
        {
            var query = _db.Tasks
                .AsNoTracking()
                .Include(x => x.Assignments)
                .ThenInclude(a => a.User)
                .Where(x => x.ProjectId == projectId);

            if (columnId.HasValue)
            {
                query = query.Where(x => x.ColumnId == columnId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(searchLower) || (x.Description != null && x.Description.ToLower().Contains(searchLower)));
            }

            if (priority.HasValue)
            {
                query = query.Where(x => x.Priority == priority.Value);
            }

            if (assigneeId.HasValue)
            {
                if (assigneeId.Value == Guid.Empty)
                {
                    query = query.Where(x => !x.Assignments.Any());
                }
                else
                {
                    query = query.Where(x => x.Assignments.Any(a => a.UserId == assigneeId.Value));
                }
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }

        public async Task<TaskStatsResponse> GetTaskStatsByProjectIdAsync(Guid projectId)
        {
            var columns = await _db.ProjectColumns
                .AsNoTracking()
                .Where(c => c.ProjectId == projectId)
                .OrderBy(c => c.Order)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.IsCompletedStage,
                    TaskCount = c.Tasks.Count()
                })
                .ToListAsync();

            var response = new TaskStatsResponse
            {
                TotalTasks = columns.Sum(c => c.TaskCount),
                CompletedTasks = columns.Where(c => c.IsCompletedStage).Sum(c => c.TaskCount),
                ColumnStats = columns.Select(c => new ColumnStat
                {
                    ColumnId = c.Id,
                    ColumnName = c.Name,
                    TaskCount = c.TaskCount
                }).ToList()
            };
            return response;
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
