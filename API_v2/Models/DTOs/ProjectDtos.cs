using System.ComponentModel.DataAnnotations;

namespace API_v2.Models.DTOs
{
    public class CreateProjectRequest
    {
        [Required(ErrorMessage = "Project name is required")]
        [MaxLength(200, ErrorMessage = "Project name must not exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters")]
        public string? Description { get; set; }
    }

    public class UpdateProjectRequest
    {
        [Required(ErrorMessage = "Project name is required")]
        [MaxLength(200, ErrorMessage = "Project name must not exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters")]
        public string? Description { get; set; }
    }

    public class ProjectResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid OwnerId { get; set; }
        public string? OwnerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UserRole { get; set; }
        public int MemberCount { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
    }

    public class AddMemberRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [AllowedValues("Owner", "Manager", "Member", ErrorMessage = "Role must be one of: Owner, Manager, Member")]
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateMemberRequest
    {
        [Required(ErrorMessage = "Role is required")]
        [AllowedValues("Owner", "Manager", "Member", ErrorMessage = "Role must be one of: Owner, Manager, Member")]
        public string Role { get; set; } = string.Empty;
    }

    public class MemberResponse
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
