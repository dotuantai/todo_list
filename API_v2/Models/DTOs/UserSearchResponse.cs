namespace API_v2.Models.DTOs
{
    public class UserSearchResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
