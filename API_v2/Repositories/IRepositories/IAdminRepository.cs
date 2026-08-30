namespace API_v2.Repositories.IRepositories;

public sealed record AdminTotals(int Users, int Projects, int Tasks);
public sealed record TaskStatusCounts(int ToDo, int InProgress, int Done);
public sealed record DailyRegistrationCount(DateTime Date, int Count);
public sealed record ProjectHealthSummary(string ProjectName, int ToDo, int InProgress, int Done, int Overdue);

public interface IAdminRepository
{
    Task<AdminTotals> GetTotalsAsync();
    Task<TaskStatusCounts> GetTaskStatusCountsAsync();
    Task<(List<DailyRegistrationCount> Projects, List<DailyRegistrationCount> Users)> GetRegistrationsAsync(DateTime since);
    Task<List<ProjectHealthSummary>> GetTopProjectHealthAsync(DateTime now, int limit);
}
