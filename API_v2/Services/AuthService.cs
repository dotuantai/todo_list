using Microsoft.Extensions.Logging;
using API_v2.Exceptions;
using API_v2.Helpers;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;
using API_v2.Datas;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth;

namespace API_v2.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailQueue _emailQueue;
        private readonly IMemoryCache _memoryCache;
        private readonly string _googleClientId;

        public AuthService(
            IUserRepository userRepo, 
            IRefreshTokenRepository refreshTokenRepo, 
            JwtHelper jwtHelper,
            ILogger<AuthService> logger,
            IEmailQueue emailQueue,
            IMemoryCache memoryCache,
            IConfiguration configuration)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _jwtHelper = jwtHelper;
            _logger = logger;
            _emailQueue = emailQueue;
            _memoryCache = memoryCache;
            _googleClientId = configuration["Google:ClientId"] ?? string.Empty;
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

        public async Task RegisterAsync(RegisterRequest req)
        {
            var emailLower = req.Email.Trim().ToLower();
            
            // Password validation
            if (!IsStrongPassword(req.Password))
            {
                throw ApiException.BadRequest("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
            }

            var existingUser = await _userRepo.GetByEmailAsync(emailLower);
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
                    CreatedAt = DateTime.UtcNow,
                    RoleId = Guid.Parse("33333333-3333-3333-3333-333333333333") // Member Role
                };
                _userRepo.Create(user);
            }

            // Generate secure OTP
            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            
            // Save to memory cache (valid for 5 minutes)
            _memoryCache.Set($"OTP_{emailLower}", otp, TimeSpan.FromMinutes(5));
            await _userRepo.SaveAsync();

            // Send Email
            var subject = "TutaFlow - Verification Code";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                    <h2 style='color: #4f46e5; text-align: center;'>Welcome to TutaFlow</h2>
                    <p>Thank you for registering. Please use the following One-Time Password (OTP) to verify your account. This code is valid for 5 minutes.</p>
                    <div style='background-color: #f8fafc; border: 1px dashed #cbd5e1; padding: 15px; text-align: center; margin: 20px 0;'>
                        <span style='font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #1e293b;'>{otp}</span>
                    </div>
                    <p style='font-size: 12px; color: #64748b; text-align: center;'>If you did not request this code, you can safely ignore this email.</p>
                </div>";

            _emailQueue.QueueEmail(emailLower, subject, body);
            _logger.LogInformation("AUDIT [Register Initialized] OTP queued for Email: {Email}", emailLower);
        }

        public async Task VerifyOtpAsync(VerifyOtpRequest req)
        {
            var emailLower = req.Email.Trim().ToLower();

            if (!_memoryCache.TryGetValue($"OTP_{emailLower}", out string? storedOtp) || storedOtp != req.Otp.Trim())
            {
                throw ApiException.BadRequest("Invalid or expired OTP code.");
            }

            var user = await _userRepo.GetByEmailAsync(emailLower);
            if (user == null)
            {
                throw ApiException.NotFound("User not found.");
            }

            user.IsActive = true;
            _memoryCache.Remove($"OTP_{emailLower}");
            await _userRepo.SaveAsync();

            _logger.LogInformation("AUDIT [Email Verified] User ID: {UserId}, Email: {Email} has been activated.", user.Id, emailLower);
        }

        public async Task ResendOtpAsync(string email)
        {
            var emailLower = email.Trim().ToLower();
            var user = await _userRepo.GetByEmailAsync(emailLower);

            if (user == null)
            {
                throw ApiException.NotFound("User not found.");
            }

            if (user.IsActive)
            {
                throw ApiException.BadRequest("Account is already active.");
            }

            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            // Save to memory cache (valid for 5 minutes)
            _memoryCache.Set($"OTP_{emailLower}", otp, TimeSpan.FromMinutes(5));

            var subject = "TutaFlow - Verification Code";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                    <h2 style='color: #4f46e5; text-align: center;'>Welcome to TutaFlow</h2>
                    <p>Please use the following One-Time Password (OTP) to verify your account. This code is valid for 5 minutes.</p>
                    <div style='background-color: #f8fafc; border: 1px dashed #cbd5e1; padding: 15px; text-align: center; margin: 20px 0;'>
                        <span style='font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #1e293b;'>{otp}</span>
                    </div>
                    <p style='font-size: 12px; color: #64748b; text-align: center;'>If you did not request this code, you can safely ignore this email.</p>
                </div>";

            _emailQueue.QueueEmail(emailLower, subject, body);
            _logger.LogInformation("AUDIT [OTP Resent] Email queue request submitted for: {Email}", emailLower);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest req)
        {
            var emailLower = req.Email?.Trim().ToLower() ?? string.Empty;
            var user = await _userRepo.GetByEmailAsync(emailLower);
            
            if (user is null)
            {
                _logger.LogWarning("SECURITY AUDIT [Login Failed] User not found: {Email}", emailLower);
                throw ApiException.Unauthorized("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("SECURITY AUDIT [Login Failed] Deactivated account attempt: {Email} (ID: {UserId})", emailLower, user.Id);
                throw ApiException.Unauthorized("Invalid email or password.");
            }

            if (!PasswordHelper.VerifyPassword(req.Password, user.PasswordHash))
            {
                _logger.LogWarning("SECURITY AUDIT [Login Failed] Incorrect password: {Email} (ID: {UserId})", emailLower, user.Id);
                throw ApiException.Unauthorized("Invalid email or password.");
            }

            var activeTokens = await _refreshTokenRepo.GetActiveTokensByUserIdAsync(user.Id);
            foreach (var token in activeTokens)
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
            await _refreshTokenRepo.SaveAsync();

            _logger.LogInformation("AUDIT [Login Success] User logged in: {Email} (ID: {UserId})", emailLower, user.Id);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RequiresPasswordChange = user.RequiresPasswordChange
            };
        }

        public async Task<LoginResponse> RefreshAsync(string refreshToken)
        {
            var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken);
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

            var user = await _userRepo.GetByIdAsync(token.UserId);
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

        public async Task LogoutAsync(string refreshToken)
        {
            var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken);
            if (token is null)
            {
                _logger.LogWarning("AUDIT [Logout Attempt] Refresh token not found on database for revocation.");
                return;
            }

            token.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepo.SaveAsync();

            _logger.LogInformation("AUDIT [Logout Success] Revoked token ID: {TokenId} for User ID: {UserId}", token.Id, token.UserId);
        }

        public async Task<List<UserSearchResponse>> SearchUsersAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<UserSearchResponse>();
            }

            return await _userRepo.SearchUsersAsync(keyword);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest req)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                throw ApiException.NotFound("User not found.");
            }

            if (!PasswordHelper.VerifyPassword(req.CurrentPassword, user.PasswordHash))
            {
                throw ApiException.BadRequest("Current password is incorrect.");
            }

            if (!IsStrongPassword(req.NewPassword))
            {
                throw ApiException.BadRequest("New password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
            }

            user.PasswordHash = PasswordHelper.HashPassword(req.NewPassword);
            user.RequiresPasswordChange = false;
            await _userRepo.SaveAsync();

            _logger.LogInformation("AUDIT [Password Changed] User ID: {UserId} successfully changed password.", userId);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest req)
        {
            var emailLower = req.Email.Trim().ToLower();
            var user = await _userRepo.GetByEmailAsync(emailLower);

            if (user == null || !user.IsActive)
            {
                throw ApiException.NotFound("User not found or account is inactive.");
            }

            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            
            // Save to memory cache (valid for 5 minutes)
            _memoryCache.Set($"PWD_RESET_OTP_{emailLower}", otp, TimeSpan.FromMinutes(5));

            var subject = "TutaFlow - Password Reset Code";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                    <h2 style='color: #4f46e5; text-align: center;'>Password Reset Request</h2>
                    <p>We received a request to reset your TutaFlow password. Please use the following One-Time Password (OTP) to reset it. This code is valid for 5 minutes.</p>
                    <div style='background-color: #f8fafc; border: 1px dashed #cbd5e1; padding: 15px; text-align: center; margin: 20px 0;'>
                        <span style='font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #1e293b;'>{otp}</span>
                    </div>
                    <p style='font-size: 12px; color: #64748b; text-align: center;'>If you did not request this, you can safely ignore this email.</p>
                </div>";

            _emailQueue.QueueEmail(emailLower, subject, body);
            _logger.LogInformation("AUDIT [Forgot Password] OTP queued for Email: {Email}", emailLower);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest req)
        {
            var emailLower = req.Email.Trim().ToLower();

            if (!_memoryCache.TryGetValue($"PWD_RESET_OTP_{emailLower}", out string? storedOtp) || storedOtp != req.Otp.Trim())
            {
                throw ApiException.BadRequest("Invalid or expired OTP code.");
            }

            if (!IsStrongPassword(req.NewPassword))
            {
                throw ApiException.BadRequest("New password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
            }

            var user = await _userRepo.GetByEmailAsync(emailLower);
            if (user == null || !user.IsActive)
            {
                throw ApiException.NotFound("User not found or account is inactive.");
            }

            user.PasswordHash = PasswordHelper.HashPassword(req.NewPassword);
            user.RequiresPasswordChange = false;
            
            _memoryCache.Remove($"PWD_RESET_OTP_{emailLower}");
            await _userRepo.SaveAsync();

            // Revoke all refresh tokens
            var activeTokens = await _refreshTokenRepo.GetActiveTokensByUserIdAsync(user.Id);
            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
            await _refreshTokenRepo.SaveAsync();

            _logger.LogInformation("AUDIT [Password Reset] Password has been reset for User ID: {UserId}, Email: {Email}", user.Id, emailLower);
        }

        public async Task<LoginResponse> LoginWithGoogleAsync(GoogleLoginRequest req)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _googleClientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken, settings);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning("SECURITY AUDIT [Google Login Failed] Invalid Google JWT: {Message}", ex.Message);
                throw ApiException.Unauthorized("Invalid or expired Google Token.");
            }

            if (!payload.EmailVerified)
            {
                _logger.LogWarning("SECURITY AUDIT [Google Login Failed] Unverified Google email: {Email}", payload.Email);
                throw ApiException.BadRequest("Google account email is not verified.");
            }

            var emailLower = payload.Email.Trim().ToLower();
            var user = await _userRepo.GetByEmailAsync(emailLower);

            if (user == null)
            {
                _logger.LogWarning("SECURITY AUDIT [Google Login Failed] Unregistered user attempt: {Email}", emailLower);
                throw ApiException.Unauthorized("Tài khoản của bạn chưa được đăng ký trong hệ thống. Vui lòng liên hệ quản trị viên.");
            }
            else
            {
                if (!user.IsActive)
                {
                    _logger.LogWarning("SECURITY AUDIT [Google Login Failed] Attempt to login to an inactive account: {Email}", emailLower);
                    throw ApiException.Unauthorized("Account is disabled or not activated.");
                }
            }

            var activeTokens = await _refreshTokenRepo.GetActiveTokensByUserIdAsync(user.Id);
            foreach (var token in activeTokens)
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
            await _refreshTokenRepo.SaveAsync();

            _logger.LogInformation("AUDIT [Google Login Success] User logged in: {Email} (ID: {UserId})", emailLower, user.Id);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RequiresPasswordChange = user.RequiresPasswordChange
            };
        }
    }
}
