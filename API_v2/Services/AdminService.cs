using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;
using System.Text.Json;
using API_v2.Models;

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
            var totalUsers = _context.Users.Count();
            var totalProjects = _context.Projects.Count();
            var totalTasks = _context.Tasks.Count();

            var response = new AdminDashboardResponse
            {
                TotalUsers = totalUsers,
                TotalProjects = totalProjects,
                TotalTasks = totalTasks,
                TaskStatusDistribution = new TaskStatusDistribution(),
                ProjectsOverTime = new List<ProjectRegistrationData>(),
                UsersOverTime = new List<UserRegistrationData>()
            };

            // Task Distribution
            var taskDist = _context.Tasks
                .Select(t => new { t.Column.IsCompletedStage, t.Column.Order })
                .ToList();

            foreach (var t in taskDist)
            {
                if (t.IsCompletedStage) response.TaskStatusDistribution.Done++;
                else if (t.Order == 0) response.TaskStatusDistribution.ToDo++;
                else response.TaskStatusDistribution.InProgress++;
            }

            // Group by Date for the last 7 days
            var today = DateTime.UtcNow.Date;
            
            // Bring dates into memory to group easily
            var projectDates = _context.Projects
                .Where(p => p.CreatedAt >= today.AddDays(-6))
                .Select(p => p.CreatedAt.Date)
                .ToList();

            var userDates = _context.Users
                .Where(u => u.CreatedAt >= today.AddDays(-6))
                .Select(u => u.CreatedAt.Date)
                .ToList();

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dateStr = date.ToString("dd/MM");

                var projectsOnDate = projectDates.Count(d => d == date);
                var usersOnDate = userDates.Count(d => d == date);

                response.ProjectsOverTime.Add(new ProjectRegistrationData { Date = dateStr, Count = projectsOnDate });
                response.UsersOverTime.Add(new UserRegistrationData { Date = dateStr, Count = usersOnDate });
                response.Last7Days.Add(dateStr);
            }
            response.Last7Days.Reverse(); // Older to newer

            // Project Health Chart (Top 15 active projects)
            var topProjects = _context.Projects
                .OrderByDescending(p => p.Tasks.Count)
                .Take(15)
                .Select(p => new { 
                    p.Id, 
                    p.Name, 
                    Tasks = p.Tasks.Select(t => new { 
                        t.Deadline,
                        IsCompletedStage = t.Column != null ? t.Column.IsCompletedStage : false,
                        Order = t.Column != null ? t.Column.Order : 0
                    }).ToList() 
                })
                .ToList();

            var now = DateTime.UtcNow;

            foreach (var p in topProjects)
            {
                int todo = 0;
                int inProgress = 0;
                int done = 0;
                int overdue = 0;

                foreach(var task in p.Tasks)
                {
                    if (task.IsCompletedStage)
                    {
                        done++;
                    }
                    else
                    {
                        if (task.Deadline != default && task.Deadline < now)
                        {
                            overdue++;
                        }
                        else if (task.Order == 0)
                        {
                            todo++;
                        }
                        else
                        {
                            inProgress++;
                        }
                    }
                }

                response.ProjectHealthList.Add(new ProjectHealthData
                {
                    ProjectName = p.Name,
                    ToDo = todo,
                    InProgress = inProgress,
                    Done = done,
                    Overdue = overdue
                });
            }

            return await Task.FromResult(response);
        }
    }
}
