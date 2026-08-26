using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;

        public RefreshTokenRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _db.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task<RefreshToken?> GetActiveTokenByUserIdAsync(Guid userId)
        {
            return await _db.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId)
        {
            return await _db.RefreshTokens.AsNoTracking().Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow).ToListAsync();
        }

        public async Task<bool> TryRevokeAsync(Guid tokenId, DateTime revokedAt)
        {
            var affectedRows = await _db.RefreshTokens
                .Where(x => x.Id == tokenId && x.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, revokedAt));

            return affectedRows == 1;
        }

        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            await _db.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, DateTime.UtcNow));
        }

        public void Add(RefreshToken token)
        {
            _db.RefreshTokens.Add(token);
        }

        public async Task DeleteExpiredTokensAsync(DateTime cutoff)
        {
            await _db.RefreshTokens
                .Where(t => t.CreatedAt < cutoff)
                .ExecuteDeleteAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
