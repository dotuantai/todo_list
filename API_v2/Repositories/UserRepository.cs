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
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == lower);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _db.Users.FindAsync(id);
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
    }
}
