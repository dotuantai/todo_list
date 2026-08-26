using API_v2.Models;
using API_v2.Models.Enums;

namespace API_v2.Repositories.IRepositories
{
    public interface IOtpRepository
    {
        void Add(Otp otp);
        Task<Otp?> GetLatestValidOtpAsync(string email, OtpType type);
        Task InvalidateActiveOtpsAsync(string email, OtpType type);
        Task DeleteExpiredOtpsAsync(DateTime cutoff);
        Task SaveAsync();
    }
}
