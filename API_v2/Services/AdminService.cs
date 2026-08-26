using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;
using System.Text.Json;
using API_v2.Models;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Services
{
    public class AdminService : IAdminService
    {
        private readonly Datas.AppDbContext _context;

        public AdminService(Datas.AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardResponse> GetDashboardStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalProjects = await _context.Projects.CountAsync();
            var totalTasks = await _context.Tasks.CountAsync();

            var response = new AdminDashboardResponse
            {
                TotalUsers = totalUsers,
                TotalProjects = totalProjects,
                TotalTasks = totalTasks,
                TaskStatusDistribution = new TaskStatusDistribution(),
                ProjectsOverTime = new List<ProjectRegistrationData>(),
                UsersOverTime = new List<UserRegistrationData>()
            };

            var taskDistribution = await _context.Tasks
                .GroupBy(_ => 1)
                .Select(group => new TaskStatusDistribution
                {
                    Done = group.Count(task => task.Column.IsCompletedStage),
                    ToDo = group.Count(task => !task.Column.IsCompletedStage && task.Column.Order == 0),
                    InProgress = group.Count(task => !task.Column.IsCompletedStage && task.Column.Order != 0)
                })
                .FirstOrDefaultAsync();
            response.TaskStatusDistribution = taskDistribution ?? new TaskStatusDistribution();

            // Group by Date for the last 7 days
            var today = DateTime.UtcNow.Date;
            
            var projectCountsByDate = await _context.Projects
                .Where(p => p.CreatedAt >= today.AddDays(-6))
                .GroupBy(p => p.CreatedAt.Date)
                .Select(group => new { Date = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.Date, item => item.Count);

            var userCountsByDate = await _context.Users
                .Where(u => u.CreatedAt >= today.AddDays(-6))
                .GroupBy(u => u.CreatedAt.Date)
                .Select(group => new { Date = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.Date, item => item.Count);

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dateStr = date.ToString("dd/MM");

                var projectsOnDate = projectCountsByDate.GetValueOrDefault(date);
                var usersOnDate = userCountsByDate.GetValueOrDefault(date);

                response.ProjectsOverTime.Add(new ProjectRegistrationData { Date = dateStr, Count = projectsOnDate });
                response.UsersOverTime.Add(new UserRegistrationData { Date = dateStr, Count = usersOnDate });
                response.Last7Days.Add(dateStr);
            }

            // Project Health Chart (Top 15 active projects)
            var topProjects = await _context.Projects
                .AsNoTracking()
                .OrderByDescending(p => p.Tasks.Count)
                .Take(15)
                .Select(p => new
                {
                    p.Id,
                    p.Name
                })
                .ToListAsync();

            var now = DateTime.UtcNow;
            var topProjectIds = topProjects.Select(project => project.Id).ToList();
            var healthByProject = await _context.Tasks
                .Where(task => task.ProjectId.HasValue && topProjectIds.Contains(task.ProjectId.Value))
                .GroupBy(task => task.ProjectId!.Value)
                .Select(group => new
                {
                    ProjectId = group.Key,
                    Done = group.Count(task => task.Column.IsCompletedStage),
                    Overdue = group.Count(task => !task.Column.IsCompletedStage && task.Deadline < now),
                    ToDo = group.Count(task => !task.Column.IsCompletedStage && task.Deadline >= now && task.Column.Order == 0),
                    InProgress = group.Count(task => !task.Column.IsCompletedStage && task.Deadline >= now && task.Column.Order != 0)
                })
                .ToDictionaryAsync(item => item.ProjectId);

            foreach (var project in topProjects)
            {
                healthByProject.TryGetValue(project.Id, out var health);

                response.ProjectHealthList.Add(new ProjectHealthData
                {
                    ProjectName = project.Name,
                    ToDo = health?.ToDo ?? 0,
                    InProgress = health?.InProgress ?? 0,
                    Done = health?.Done ?? 0,
                    Overdue = health?.Overdue ?? 0
                });
            }

            return response;
        }
    }
}
