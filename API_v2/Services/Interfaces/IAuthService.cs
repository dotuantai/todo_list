using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest req);
        Task VerifyOtpAsync(VerifyOtpRequest req);
        Task ResendOtpAsync(string email);
        Task<LoginResponse> LoginAsync(LoginRequest req);
        Task<LoginResponse> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task<List<UserSearchResponse>> SearchUsersAsync(string keyword);
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequest req);
    }
}
