using Microsoft.Extensions.Logging;
using API_v2.Exceptions;
using API_v2.Helpers;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositorys.IRepositorys;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepo, 
            IRefreshTokenRepository refreshTokenRepo, 
            JwtHelper jwtHelper,
            ILogger<AuthService> logger)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _jwtHelper = jwtHelper;
            _logger = logger;
        }

        public void Register(RegisterRequest req)
        {
            var emailLower = req.Email.Trim().ToLower();
            if (_userRepo.GetByEmail(emailLower) is not null)
            {
                _logger.LogWarning("AUDIT [Register Failed] Email: {Email} already exists.", emailLower);
                throw ApiException.Conflict("Email is already in use.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = emailLower,
                PasswordHash = PasswordHelper.HashPassword(req.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _userRepo.Create(user);
            _userRepo.Save();

            _logger.LogInformation("AUDIT [Register Success] User Created ID: {UserId}, Email: {Email}", user.Id, emailLower);
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
