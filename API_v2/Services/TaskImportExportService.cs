using API_v2.Exceptions;
using API_v2.Models;
using API_v2.Models.Constants;
using API_v2.Models.Enums;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;
using MiniExcelLibs;

namespace API_v2.Services;

public class TaskImportExportService : ITaskImportExportService
{
    private const int MaxImportRows = 1000;
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectColumnRepository _columnRepository;

    public TaskImportExportService(ITaskRepository taskRepository, IProjectRepository projectRepository,
        IProjectColumnRepository columnRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _columnRepository = columnRepository;
    }

    public async Task<byte[]> GetTaskTemplateAsync()
    {
        var template = new List<dynamic>
        {
            new { Title = "Example Task 1", Description = "Description of task 1", Deadline = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-dd"), StartDate = DateTime.UtcNow.ToString("yyyy-MM-dd"), EstimatedHours = 4.5, Priority = "High" },
            new { Title = "Example Task 2", Description = "Description of task 2", Deadline = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"), StartDate = DateTime.UtcNow.ToString("yyyy-MM-dd"), EstimatedHours = 2.0, Priority = "Medium" }
        };
        using var stream = new MemoryStream();
        await stream.SaveAsAsync(template);
        return stream.ToArray();
    }

    public async Task<int> ImportTasksAsync(Guid projectId, Guid currentUserId, Stream fileStream, string fileName)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            throw ApiException.BadRequest("Only .xlsx task import files are supported.");
        if (!fileStream.CanRead) throw ApiException.BadRequest("The uploaded task file is not readable.");

        if (!await _projectRepository.IsSystemAdminAsync(currentUserId))
        {
            var member = await _projectRepository.GetMemberAsync(projectId, currentUserId);
            if (member is null || !ProjectRoles.IsOwnerOrManager(member.Role))
                throw ApiException.Forbidden("Only Owners or Managers can import tasks.");
        }

        var defaultColumn = (await _columnRepository.GetColumnsByProjectIdAsync(projectId))
            .OrderBy(column => column.Order).FirstOrDefault()
            ?? throw ApiException.BadRequest("Project has no columns to assign tasks to.");
        var importedTasks = new List<TodoTask>();
        var rowNumber = 0;
        foreach (var row in await fileStream.QueryAsync(useHeaderRow: true))
        {
            if (++rowNumber > MaxImportRows)
                throw ApiException.BadRequest($"Task import is limited to {MaxImportRows} rows per file.");
            if (row is not IDictionary<string, object> values) continue;
            var title = values.TryGetValue("Title", out var titleValue) ? titleValue?.ToString() : null;
            if (string.IsNullOrWhiteSpace(title)) continue;
            if (!values.TryGetValue("Deadline", out var deadlineValue) || !DateTime.TryParse(deadlineValue?.ToString(), out var deadline))
                throw ApiException.BadRequest($"Task '{title}' is missing a valid Deadline.");
            if (!values.TryGetValue("StartDate", out var startValue) || !DateTime.TryParse(startValue?.ToString(), out var startDate))
                throw ApiException.BadRequest($"Task '{title}' is missing a valid StartDate.");
            deadline = NormalizeToUtc(deadline);
            startDate = NormalizeToUtc(startDate);
            if (startDate > deadline) throw ApiException.BadRequest($"Task '{title}' has a start date after its deadline.");
            double? estimatedHours = null;
            if (values.TryGetValue("EstimatedHours", out var hoursValue) && double.TryParse(hoursValue?.ToString(), out var hours))
            {
                if (hours < 0) throw ApiException.BadRequest($"Task '{title}' has invalid estimated hours.");
                estimatedHours = hours;
            }
            var priorityText = values.TryGetValue("Priority", out var priorityValue) ? priorityValue?.ToString() : null;
            if (!Enum.TryParse<TaskPriority>(priorityText, true, out var priority) || !Enum.IsDefined(priority)) priority = TaskPriority.Medium;
            importedTasks.Add(new TodoTask
            {
                Title = title.Trim(), Description = values.TryGetValue("Description", out var description) ? description?.ToString()?.Trim() : null,
                CreatedAt = DateTime.UtcNow, CreatorId = currentUserId, Deadline = deadline, StartDate = startDate,
                EstimatedHours = estimatedHours, ColumnId = defaultColumn.Id, ProjectId = projectId, Priority = priority
            });
        }
        if (importedTasks.Count == 0)
            throw ApiException.BadRequest("No valid tasks found in the uploaded file. Ensure 'Title' column exists.");
        foreach (var task in importedTasks) _taskRepository.Add(task);
        await _taskRepository.SaveAsync();
        return importedTasks.Count;
    }

    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => value
    };
}
