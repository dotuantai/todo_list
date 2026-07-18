using API_v2.Models;

namespace API_v2.Repositorys.IRepositorys
{
    public interface IRefreshTokenRepository
    {
        RefreshToken? GetByToken(string token);
        RefreshToken? GetActiveTokenByUserId(Guid userId);
        List<RefreshToken> GetActiveTokensByUserId(Guid userId);
        void Add(RefreshToken token);
        void Save();
    }
}
