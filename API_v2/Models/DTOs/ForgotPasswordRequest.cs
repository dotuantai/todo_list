using System.ComponentModel.DataAnnotations;

namespace API_v2.Models.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
