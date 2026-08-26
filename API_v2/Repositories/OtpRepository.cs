using API_v2.Datas;
using API_v2.Models;
using API_v2.Models.Enums;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly AppDbContext _db;

        public OtpRepository(AppDbContext db)
        {
            _db = db;
        }

        public void Add(Otp otp) => _db.Otps.Add(otp);

        public Task<Otp?> GetLatestValidOtpAsync(string email, OtpType type)
        {
            var now = DateTime.UtcNow;
            return _db.Otps
                .Where(otp =>
                    otp.Email == email &&
                    otp.Type == type &&
                    !otp.IsUsed &&
                    otp.AttemptsCount < 5 &&
                    otp.ExpiresAt > now)
                .OrderByDescending(otp => otp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public Task InvalidateActiveOtpsAsync(string email, OtpType type)
        {
            return _db.Otps
                .Where(otp => otp.Email == email && otp.Type == type && !otp.IsUsed)
                .ExecuteUpdateAsync(setters => setters.SetProperty(otp => otp.IsUsed, true));
        }

        public Task DeleteExpiredOtpsAsync(DateTime cutoff)
        {
            return _db.Otps
                .Where(otp => otp.ExpiresAt < cutoff)
                .ExecuteDeleteAsync();
        }

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}
