using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models;
using API_v2.Models.DTOs;

namespace API_v2.Repositories.IRepositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        void Create(User user);
        Task SaveAsync();
        Task<List<UserSearchResponse>> SearchUsersAsync(string keyword);
        Task<List<AdminUserResponse>> GetAllUsersAsync();
        Task<Guid?> GetRoleIdByNameAsync(string roleName);
        Task<bool> UpdateUserRoleAsync(Guid userId, string newRole);
        Task<bool> UpdateUserStatusAsync(Guid userId, bool isActive);
    }
}
