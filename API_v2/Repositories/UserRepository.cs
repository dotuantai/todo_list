using API_v2.Datas;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var lower = email.Trim().ToLower();
            return await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == lower);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public void Create(User user)
        {
            _db.Users.Add(user);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<List<UserSearchResponse>> SearchUsersAsync(string keyword)
        {
            var lower = keyword.Trim().ToLower();
            return await _db.Users
                .Where(u => u.IsActive && u.Email.Contains(lower))
                .OrderBy(u => u.Email)
                .Take(10)
                .Select(u => new UserSearchResponse
                {
                    UserId = u.Id,
                    Email = u.Email
                })
                .ToListAsync();
        }

        public async Task<List<AdminUserResponse>> GetAllUsersAsync()
        {
            return await _db.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminUserResponse
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role != null ? u.Role.Name : "Member",
                    IsActive = u.IsActive,
                    RequiresPasswordChange = u.RequiresPasswordChange,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<Guid?> GetRoleIdByNameAsync(string roleName)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            return role?.Id;
        }

        public async Task<bool> UpdateUserRoleAsync(Guid userId, string newRole)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == newRole);
            if (role == null) return false;

            user.RoleId = role.Id;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserStatusAsync(Guid userId, bool isActive)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = isActive;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
