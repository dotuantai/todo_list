using System.ComponentModel.DataAnnotations;

namespace API_v2.Models.DTOs
{
    public class CreateUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
