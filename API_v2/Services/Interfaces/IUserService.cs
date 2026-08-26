using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResponse<AdminUserResponse>> GetUsersAsync(int page, int pageSize);
        Task CreateUserAsync(CreateUserRequest request);
        Task ResetTemporaryPasswordAsync(Guid userId);
        Task UpdateUserRoleAsync(Guid userId, string role);
        Task UpdateUserStatusAsync(Guid userId, bool isActive);
    }
}
