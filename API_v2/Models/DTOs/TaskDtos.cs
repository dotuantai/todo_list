using System.ComponentModel.DataAnnotations;

namespace API_v2.Models.DTOs
{
    public class CreateTaskRequest
    {
        [Required(ErrorMessage = "Task title is required")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateTaskRequest
    {
        [Required(ErrorMessage = "Task id is required")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Status { get; set; }
    }

    public class AssignTaskRequest
    {
        [Required(ErrorMessage = "Task id is required")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "User id is required")]
        public Guid UserId { get; set; }

        public bool CanView { get; set; } = true;
        public bool CanEdit { get; set; } = true;
    }

    public class RemoveAssignmentRequest
    {
        [Required(ErrorMessage = "Task id is required")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "User id is required")]
        public Guid UserId { get; set; }
    }

    public class ChangeTaskStatusRequest
    {
        [Required(ErrorMessage = "Task id is required")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;
    }

    public class TaskResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class AssignedUserResponse
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
    }

    public class TaskDetailResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<AssignedUserResponse>? AssignedUsers { get; set; }
    }
}
