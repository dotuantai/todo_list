using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface IAuthService
    {
        void Register(RegisterRequest req);
        LoginResponse Login(LoginRequest req);
        LoginResponse Refresh(string refreshToken);
        void Logout(string refreshToken);
        List<UserSearchResponse> SearchUsers(string keyword);
    }
}
