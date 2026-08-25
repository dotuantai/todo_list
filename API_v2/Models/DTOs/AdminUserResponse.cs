using System;

namespace API_v2.Models.DTOs
{
    public class AdminUserResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool RequiresPasswordChange { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
