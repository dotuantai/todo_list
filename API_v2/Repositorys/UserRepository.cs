using API_v2.Datas;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositorys.IRepositorys;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositorys
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public User? GetByEmail(string email)
        {
            return _db.Users.AsNoTracking().FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        }

        public User? GetById(Guid id)
        {
            return _db.Users.Find(id);
        }

        public void Create(User user)
        {
            _db.Users.Add(user);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public List<UserSearchResponse> SearchUsers(string keyword)
        {
            var lower = keyword.Trim().ToLower();
            return _db.Users
                .Where(u => u.IsActive && u.Email.ToLower().Contains(lower))
                .OrderBy(u => u.Email)
                .Take(10)
                .Select(u => new UserSearchResponse
                {
                    UserId = u.Id,
                    Email = u.Email
                })
                .ToList();
        }
    }
}
