using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositorys.IRepositorys;

namespace API_v2.Repositorys
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;

        public RefreshTokenRepository(AppDbContext db)
        {
            _db = db;
        }

        public RefreshToken? GetByToken(string token)
        {
            return _db.RefreshTokens.FirstOrDefault(x => x.Token == token);
        }

        public RefreshToken? GetActiveTokenByUserId(Guid userId)
        {
            return _db.RefreshTokens.FirstOrDefault(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow);
        }

        public List<RefreshToken> GetActiveTokensByUserId(Guid userId)
        {
            return _db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow).ToList();
        }

        public void Add(RefreshToken token)
        {
            _db.RefreshTokens.Add(token);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
