using API_v2.Models.Enums;

namespace API_v2.Models
{
    public class Otp
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string CodeHash { get; set; } = string.Empty;
        public OtpType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public int AttemptsCount { get; set; }
    }
}
