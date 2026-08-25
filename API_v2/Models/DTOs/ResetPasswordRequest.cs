using System.ComponentModel.DataAnnotations;

namespace API_v2.Models.DTOs
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Otp { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}
