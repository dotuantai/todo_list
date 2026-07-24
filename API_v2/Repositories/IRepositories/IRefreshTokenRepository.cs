using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models;

namespace API_v2.Repositories.IRepositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken?> GetActiveTokenByUserIdAsync(Guid userId);
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);
        void Add(RefreshToken token);
        Task DeleteExpiredTokensAsync(DateTime cutoff);
        Task SaveAsync();
    }
}
