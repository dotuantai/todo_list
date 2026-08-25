namespace API_v2.Models.DTOs
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public bool RequiresPasswordChange { get; set; }
    }
}
