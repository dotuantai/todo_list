using Microsoft.Extensions.Logging;
using API_v2.Exceptions;
using API_v2.Helpers;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositorys.IRepositorys;
using API_v2.Services.Interfaces;
using API_v2.Datas;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace API_v2.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<AuthService> _logger;
        private readonly AppDbContext _db;
        private readonly IEmailQueue _emailQueue;
        private readonly IMemoryCache _memoryCache;

        public AuthService(
            IUserRepository userRepo, 
            IRefreshTokenRepository refreshTokenRepo, 
            JwtHelper jwtHelper,
            ILogger<AuthService> logger,
            AppDbContext db,
            IEmailQueue emailQueue,
            IMemoryCache memoryCache)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _jwtHelper = jwtHelper;
            _logger = logger;
            _db = db;
            _emailQueue = emailQueue;
            _memoryCache = memoryCache;
        }

        private bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;
            
            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            bool hasSpecial = false;

            foreach (var ch in password)
            {
                if (char.IsUpper(ch)) hasUpper = true;
                else if (char.IsLower(ch)) hasLower = true;
                else if (char.IsDigit(ch)) hasDigit = true;
                else if (!char.IsLetterOrDigit(ch)) hasSpecial = true;
            }

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        public void Register(RegisterRequest req)
        {
            var emailLower = req.Email.Trim().ToLower();
            
            // Password validation
            if (!IsStrongPassword(req.Password))
            {
                throw ApiException.BadRequest("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
            }

            var existingUser = _db.Users.FirstOrDefault(u => u.Email.ToLower() == emailLower);
            if (existingUser is not null)
            {
                if (existingUser.IsActive)
                {
                    _logger.LogWarning("AUDIT [Register Failed] Email: {Email} already exists and is active.", emailLower);
                    throw ApiException.Conflict("Email is already in use.");
                }
                else
                {
                    // Update password for inactive user (re-registering)
                    existingUser.PasswordHash = PasswordHelper.HashPassword(req.Password);
                    existingUser.CreatedAt = DateTime.UtcNow;
                    _db.Users.Update(existingUser);
                }
            }
            else
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = emailLower,
                    PasswordHash = PasswordHelper.HashPassword(req.Password),
                    IsActive = false, // Must verify OTP to activate
                    CreatedAt = DateTime.UtcNow
                };
                _db.Users.Add(user);
            }

            // Generate OTP
            var otp = Random.Shared.Next(100000, 999999).ToString();
            
            // Save to memory cache (valid for 5 minutes)
            _memoryCache.Set($"OTP_{emailLower}", otp, TimeSpan.FromMinutes(5));
            _db.SaveChanges();

            // Send Email
            var subject = "TaskFlow Pro - Verification Code";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                    <h2 style='color: #4f46e5; text-align: center;'>Welcome to TaskFlow Pro</h2>
                    <p>Thank you for registering. Please use the following One-Time Password (OTP) to verify your account. This code is valid for 5 minutes.</p>
                    <div style='background-color: #f8fafc; border: 1px dashed #cbd5e1; padding: 15px; text-align: center; margin: 20px 0;'>
                        <span style='font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #1e293b;'>{otp}</span>
                    </div>
                    <p style='font-size: 12px; color: #64748b; text-align: center;'>If you did not request this code, you can safely ignore this email.</p>
                </div>";

            _emailQueue.QueueEmail(emailLower, subject, body);
            _logger.LogInformation("AUDIT [Register Initialized] OTP queued for Email: {Email}", emailLower);
        }

        public void VerifyOtp(VerifyOtpRequest req)
        {
            var emailLower = req.Email.Trim().ToLower();

            if (!_memoryCache.TryGetValue($"OTP_{emailLower}", out string? storedOtp) || storedOtp != req.Otp.Trim())
            {
                throw ApiException.BadRequest("Invalid or expired OTP code.");
            }

            var user = _db.Users.FirstOrDefault(u => u.Email.ToLower() == emailLower);
            if (user == null)
            {
                throw ApiException.NotFound("User not found.");
            }

            user.IsActive = true;
            _memoryCache.Remove($"OTP_{emailLower}");
            _db.SaveChanges();

            _logger.LogInformation("AUDIT [Email Verified] User ID: {UserId}, Email: {Email} has been activated.", user.Id, emailLower);
        }

        public void ResendOtp(string email)
        {
            var emailLower = email.Trim().ToLower();
            var user = _db.Users.FirstOrDefault(u => u.Email.ToLower() == emailLower);

            if (user == null)
            {
                throw ApiException.NotFound("User not found.");
            }

            if (user.IsActive)
            {
                throw ApiException.BadRequest("Account is already active.");
            }

            var otp = Random.Shared.Next(100000, 999999).ToString();

            // Save to memory cache (valid for 5 minutes)
            _memoryCache.Set($"OTP_{emailLower}", otp, TimeSpan.FromMinutes(5));

            var subject = "TaskFlow Pro - Verification Code";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                    <h2 style='color: #4f46e5; text-align: center;'>Welcome to TaskFlow Pro</h2>
                    <p>Please use the following One-Time Password (OTP) to verify your account. This code is valid for 5 minutes.</p>
                    <div style='background-color: #f8fafc; border: 1px dashed #cbd5e1; padding: 15px; text-align: center; margin: 20px 0;'>
                        <span style='font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #1e293b;'>{otp}</span>
                    </div>
                    <p style='font-size: 12px; color: #64748b; text-align: center;'>If you did not request this code, you can safely ignore this email.</p>
                </div>";

            _emailQueue.QueueEmail(emailLower, subject, body);
            _logger.LogInformation("AUDIT [OTP Resent] Email queue request submitted for: {Email}", emailLower);
        }

        public LoginResponse Login(LoginRequest req)
        {
            var emailLower = req.Email?.Trim().ToLower() ?? string.Empty;
            var user = _userRepo.GetByEmail(emailLower);
            
            if (user is null)
            {
                _logger.LogWarning("SECURITY AUDIT [Login Failed] User not found: {Email}", emailLower);
                throw ApiException.Unauthorized("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("SECURITY AUDIT [Login Failed] Deactivated account attempt: {Email} (ID: {UserId})", emailLower, user.Id);
                throw ApiException.Forbidden("Account has been deactivated.");
            }

            if (!PasswordHelper.VerifyPassword(req.Password, user.PasswordHash))
            {
                _logger.LogWarning("SECURITY AUDIT [Login Failed] Incorrect password: {Email} (ID: {UserId})", emailLower, user.Id);
                throw ApiException.Unauthorized("Invalid email or password.");
            }

            foreach (var token in _refreshTokenRepo.GetActiveTokensByUserId(user.Id))
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            _refreshTokenRepo.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            _refreshTokenRepo.Save();

            _logger.LogInformation("AUDIT [Login Success] User logged in: {Email} (ID: {UserId})", emailLower, user.Id);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public LoginResponse Refresh(string refreshToken)
        {
            var token = _refreshTokenRepo.GetByToken(refreshToken);
            if (token is null)
            {
                _logger.LogWarning("SECURITY AUDIT [Refresh Failed] Refresh token was not found on database.");
                throw ApiException.Unauthorized("Invalid refresh token.");
            }

            if (token.RevokedAt.HasValue)
            {
                _logger.LogWarning("SECURITY AUDIT [Refresh Failed] Re-used/Revoked refresh token attempt: {TokenId} for User ID: {UserId}.", token.Id, token.UserId);
                throw ApiException.Unauthorized("Refresh token has been revoked. Please sign in again.");
            }

            if (token.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("AUDIT [Refresh Failed] Expired refresh token: {TokenId} for User ID: {UserId}.", token.Id, token.UserId);
                throw ApiException.Unauthorized("Refresh token has expired. Please sign in again.");
            }

            var user = _userRepo.GetById(token.UserId);
            if (user is null || !user.IsActive)
            {
                _logger.LogWarning("SECURITY AUDIT [Refresh Failed] Inactive user refresh attempt: User ID: {UserId}.", token.UserId);
                throw ApiException.Forbidden("Account is no longer active.");
            }

            _logger.LogInformation("AUDIT [Refresh Success] Refreshed AccessToken for User ID: {UserId}", user.Id);

            return new LoginResponse
            {
                AccessToken = _jwtHelper.GenerateAccessToken(user)
            };
        }

        public void Logout(string refreshToken)
        {
            var token = _refreshTokenRepo.GetByToken(refreshToken);
            if (token is null)
            {
                _logger.LogWarning("AUDIT [Logout Attempt] Refresh token not found on database for revocation.");
                return;
            }

            token.RevokedAt = DateTime.UtcNow;
            _refreshTokenRepo.Save();

            _logger.LogInformation("AUDIT [Logout Success] Revoked token ID: {TokenId} for User ID: {UserId}", token.Id, token.UserId);
        }

        public List<UserSearchResponse> SearchUsers(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<UserSearchResponse>();
            }

            return _userRepo.SearchUsers(keyword);
        }
    }
}
