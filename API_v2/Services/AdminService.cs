using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<AdminDashboardResponse> GetDashboardStatsAsync()
        {
            var totals = await _adminRepository.GetTotalsAsync();

            var response = new AdminDashboardResponse
            {
                TotalUsers = totals.Users,
                TotalProjects = totals.Projects,
                TotalTasks = totals.Tasks,
                TaskStatusDistribution = new TaskStatusDistribution(),
                ProjectsOverTime = new List<ProjectRegistrationData>(),
                UsersOverTime = new List<UserRegistrationData>()
            };

            var taskDistribution = await _adminRepository.GetTaskStatusCountsAsync();
            response.TaskStatusDistribution = new TaskStatusDistribution
            {
                ToDo = taskDistribution.ToDo, InProgress = taskDistribution.InProgress, Done = taskDistribution.Done
            };

            // Group by Date for the last 7 days
            var today = DateTime.UtcNow.Date;
            
            var registrations = await _adminRepository.GetRegistrationsAsync(today.AddDays(-6));
            var projectCountsByDate = registrations.Projects.ToDictionary(item => item.Date, item => item.Count);
            var userCountsByDate = registrations.Users.ToDictionary(item => item.Date, item => item.Count);

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

            foreach (var project in await _adminRepository.GetTopProjectHealthAsync(DateTime.UtcNow, 15))
            {
                response.ProjectHealthList.Add(new ProjectHealthData
                {
                    ProjectName = project.ProjectName,
                    ToDo = project.ToDo, InProgress = project.InProgress,
                    Done = project.Done, Overdue = project.Overdue
                });
            }

            return response;
        }
    }
}
