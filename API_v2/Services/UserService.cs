using System.Net;
using API_v2.Exceptions;
using API_v2.Helpers;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class UserService : IUserService
    {
        private static readonly HashSet<string> AssignableRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Manager",
            "Member"
        };

        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IEmailQueue _emailQueue;

        public UserService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IEmailQueue emailQueue)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _emailQueue = emailQueue;
        }

        public Task<PagedResponse<AdminUserResponse>> GetUsersAsync(int page, int pageSize)
            => _userRepository.GetAllUsersAsync(page, pageSize);

        public async Task CreateUserAsync(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Role))
            {
                throw ApiException.BadRequest("Email, FullName, and Role are required.");
            }

            if (!AssignableRoles.Contains(request.Role))
            {
                throw ApiException.BadRequest("Only Manager or Member can be assigned.");
            }

            var email = request.Email.Trim().ToLowerInvariant();
            if (await _userRepository.GetByEmailAsync(email) is not null)
            {
                throw ApiException.BadRequest("Email is already in use.");
            }

            var roleId = await _userRepository.GetRoleIdByNameAsync(request.Role)
                ?? throw ApiException.BadRequest("Role not found.");
            var temporaryPassword = PasswordHelper.GenerateRandomPassword(12);
            var fullName = request.FullName.Trim();

            _userRepository.Create(new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(temporaryPassword),
                IsActive = true,
                RequiresPasswordChange = true,
                CreatedAt = DateTime.UtcNow,
                RoleId = roleId
            });
            await _userRepository.SaveAsync();

            _emailQueue.QueueEmail(
                email,
                "TutaFlow - Tài khoản của bạn đã được tạo",
                BuildTemporaryPasswordEmail(fullName, email, temporaryPassword, isReset: false));
        }

        public async Task ResetTemporaryPasswordAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw ApiException.NotFound("User not found.");
            var temporaryPassword = PasswordHelper.GenerateRandomPassword(12);

            user.PasswordHash = PasswordHelper.HashPassword(temporaryPassword);
            user.RequiresPasswordChange = true;
            await _userRepository.SaveAsync();
            await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);

            _emailQueue.QueueEmail(
                user.Email,
                "TutaFlow - Mật khẩu tạm thời mới",
                BuildTemporaryPasswordEmail(user.FullName ?? user.Email, user.Email, temporaryPassword, isReset: true));
        }

        public async Task UpdateUserRoleAsync(Guid userId, string role)
        {
            if (!AssignableRoles.Contains(role))
            {
                throw ApiException.BadRequest("Only Manager or Member can be assigned.");
            }

            if (!await _userRepository.UpdateUserRoleAsync(userId, role))
            {
                throw ApiException.NotFound("User not found.");
            }
        }

        public async Task UpdateUserStatusAsync(Guid userId, bool isActive)
        {
            if (!await _userRepository.UpdateUserStatusAsync(userId, isActive))
            {
                throw ApiException.NotFound("User not found.");
            }

            if (!isActive)
            {
                await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
            }
        }

        private static string BuildTemporaryPasswordEmail(
            string fullName,
            string email,
            string temporaryPassword,
            bool isReset)
        {
            var safeName = WebUtility.HtmlEncode(fullName);
            var safeEmail = WebUtility.HtmlEncode(email);
            var safePassword = WebUtility.HtmlEncode(temporaryPassword);
            var intro = isReset
                ? "Quản trị viên vừa đặt lại mật khẩu tạm thời cho tài khoản của bạn."
                : "Tài khoản của bạn đã được quản trị viên tạo thành công.";

            return $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;border:1px solid #e2e8f0;border-radius:8px">
                    <h2 style="color:#4f46e5;text-align:center">TutaFlow</h2>
                    <p>Xin chào {safeName}, {intro}</p>
                    <p><b>Email:</b> {safeEmail}</p>
                    <p><b>Mật khẩu tạm thời:</b> <code>{safePassword}</code></p>
                    <p>Bạn sẽ được yêu cầu đổi mật khẩu ngay trong lần đăng nhập tiếp theo.</p>
                </div>
                """;
        }
    }
}
