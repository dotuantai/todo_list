using System.ComponentModel.DataAnnotations;

namespace API_v2.Models.DTOs
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}
