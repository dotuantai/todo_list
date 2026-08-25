using System;
using System.Collections.Generic;

namespace API_v2.Models.DTOs
{
    public class AdminDashboardResponse
    {
        public int TotalUsers { get; set; }
        public int TotalProjects { get; set; }
        public int TotalTasks { get; set; }
        
        public TaskStatusDistribution TaskStatusDistribution { get; set; } = new TaskStatusDistribution();
        public List<ProjectRegistrationData> ProjectsOverTime { get; set; } = new List<ProjectRegistrationData>();
        public List<UserRegistrationData> UsersOverTime { get; set; } = new();
        public List<string> Last7Days { get; set; } = new();
        public List<ProjectHealthData> ProjectHealthList { get; set; } = new();
    }



    public class TaskStatusDistribution
    {
        public int ToDo { get; set; }
        public int InProgress { get; set; }
        public int Done { get; set; }
    }

    public class ProjectRegistrationData
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class UserRegistrationData
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ProjectHealthData
    {
        public string ProjectName { get; set; } = string.Empty;
        public int ToDo { get; set; }
        public int InProgress { get; set; }
        public int Done { get; set; }
        public int Overdue { get; set; }
    }
}
