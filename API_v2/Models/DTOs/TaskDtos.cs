using System.ComponentModel.DataAnnotations;
using API_v2.Models.DTOs.Validation;

namespace API_v2.Models.DTOs
{
    public class CreateTaskRequest
    {
        [Required(ErrorMessage = "Task title is required")]
        [MaxLength(500, ErrorMessage = "Task title must not exceed 500 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(5000, ErrorMessage = "Description must not exceed 5000 characters")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Deadline is required")]
        public DateTime? Deadline { get; set; }
        [Required(ErrorMessage = "Start date is required")]
        public DateTime? StartDate { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Estimated hours must be non-negative")]
        public double? EstimatedHours { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Actual hours must be non-negative")]
        public double? ActualHours { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Column ID must be greater than 0")]
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
        [Required(ErrorMessage = "Deadline is required")]
        public DateTime? Deadline { get; set; }
        [Required(ErrorMessage = "Start date is required")]
        public DateTime? StartDate { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Estimated hours must be non-negative")]
        public double? EstimatedHours { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Actual hours must be non-negative")]
        public double? ActualHours { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Column ID must be greater than 0")]
        public int ColumnId { get; set; }
        public API_v2.Models.Enums.TaskPriority Priority { get; set; } = API_v2.Models.Enums.TaskPriority.Medium;
        public List<string>? AssignedUserIds { get; set; }
    }

    public class AssignTaskRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Task ID must be greater than 0")]
        public int TaskId { get; set; }

        [ValidGuid(ErrorMessage = "User ID is not valid")]
        public Guid UserId { get; set; }
    }



    public class ChangeTaskColumnRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Task ID must be greater than 0")]
        public int TaskId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Column ID must be greater than 0")]
        public int ColumnId { get; set; }
    }

    public class TaskResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime Deadline { get; set; }
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
        public DateTime Deadline { get; set; }
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
