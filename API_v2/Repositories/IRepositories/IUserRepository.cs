using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models;

namespace API_v2.Repositories.IRepositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<Dictionary<Guid, string>> GetEmailsByIdsAsync(IEnumerable<Guid> ids);
        void Create(User user);
        Task SaveAsync();
        Task<List<User>> SearchUsersAsync(string keyword);
        Task<(List<User> Items, int TotalCount)> GetAllUsersAsync(int page, int pageSize);
        Task<Guid?> GetRoleIdByNameAsync(string roleName);
        Task<bool> UpdateUserRoleAsync(Guid userId, string newRole);
        Task<bool> UpdateUserStatusAsync(Guid userId, bool isActive);
    }
}
