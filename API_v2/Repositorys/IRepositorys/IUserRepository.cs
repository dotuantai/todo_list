using API_v2.Models;
using API_v2.Models.DTOs;

namespace API_v2.Repositorys.IRepositorys
{
    public interface IUserRepository
    {
        User? GetByEmail(string email);
        User? GetById(Guid id);
        void Create(User user);
        void Save();
        List<UserSearchResponse> SearchUsers(string keyword);
    }
}
