using System.ComponentModel.DataAnnotations;

namespace API_v2.Models.DTOs
{
    public class CreateTaskRequest
    {
        [Required(ErrorMessage = "Task title is required")]
        [MaxLength(500, ErrorMessage = "Task title must not exceed 500 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(5000, ErrorMessage = "Description must not exceed 5000 characters")]
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? StartDate { get; set; }
        public double? EstimatedHours { get; set; }
        public double? ActualHours { get; set; }
        public int ColumnId { get; set; }
        public API_v2.Models.Enums.TaskPriority Priority { get; set; } = API_v2.Models.Enums.TaskPriority.Medium;
        public string? AssigneeId { get; set; }
    }

    public class UpdateTaskRequest
    {
        [Required(ErrorMessage = "Task title is required")]
        [MaxLength(500, ErrorMessage = "Task title must not exceed 500 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(5000, ErrorMessage = "Description must not exceed 5000 characters")]
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? StartDate { get; set; }
        public double? EstimatedHours { get; set; }
        public double? ActualHours { get; set; }
        public int ColumnId { get; set; }
        public API_v2.Models.Enums.TaskPriority Priority { get; set; } = API_v2.Models.Enums.TaskPriority.Medium;
        public List<string>? AssignedUserIds { get; set; }
    }

    public class AssignTaskRequest
    {
        [Required(ErrorMessage = "Task id is required")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "User id is required")]
        public Guid UserId { get; set; }
    }



    public class ChangeTaskColumnRequest
    {
        [Required(ErrorMessage = "Task id is required")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "ColumnId is required")]
        public int ColumnId { get; set; }
    }

    public class TaskResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? StartDate { get; set; }
        public double? EstimatedHours { get; set; }
        public double? ActualHours { get; set; }
        public int ColumnId { get; set; }
        public API_v2.Models.Enums.TaskPriority Priority { get; set; } = API_v2.Models.Enums.TaskPriority.Medium;
    }

    public class AssignedUserResponse
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; }
    }

    public class TaskDetailResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? StartDate { get; set; }
        public double? EstimatedHours { get; set; }
        public double? ActualHours { get; set; }
        public int ColumnId { get; set; }
        public API_v2.Models.Enums.TaskPriority Priority { get; set; } = API_v2.Models.Enums.TaskPriority.Medium;
        public List<AssignedUserResponse>? AssignedUsers { get; set; }
    }

    public class TaskStatsResponse
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public List<ColumnStat> ColumnStats { get; set; } = new List<ColumnStat>();
    }

    public class ColumnStat
    {
        public int ColumnId { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public int TaskCount { get; set; }
    }

    public class FieldChangeDto
    {
        public string Field { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }

    public class TaskActivityResponse
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public List<FieldChangeDto> Changes { get; set; } = new();
    }
}
