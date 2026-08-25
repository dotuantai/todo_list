using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;
using API_v2.Helpers;
using System;

namespace API_v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public UserController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<AdminUserResponse>>> GetAllUsers()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return Ok(new ApiResponse<List<AdminUserResponse>>(true, "Lấy danh sách người dùng thành công.", users));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest req, [FromServices] IEmailQueue emailQueue)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.Role))
            {
                return BadRequest(new ApiResponse<object>(false, "Email, FullName, and Role are required.", null));
            }

            var emailLower = req.Email.Trim().ToLower();
            var existingUser = await _userRepo.GetByEmailAsync(emailLower);
            if (existingUser != null)
            {
                return BadRequest(new ApiResponse<object>(false, "Email is already in use.", null));
            }

            var roleId = await _userRepo.GetRoleIdByNameAsync(req.Role);
            if (roleId == null)
            {
                return BadRequest(new ApiResponse<object>(false, "Role not found.", null));
            }

            var tempPassword = PasswordHelper.GenerateRandomPassword(10);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = req.FullName,
                Email = emailLower,
                PasswordHash = PasswordHelper.HashPassword(tempPassword),
                IsActive = true,
                RequiresPasswordChange = true,
                CreatedAt = DateTime.UtcNow,
                RoleId = roleId.Value
            };

            _userRepo.Create(user);
            await _userRepo.SaveAsync();

            var subject = "TutaFlow - Tài khoản của bạn đã được tạo";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                    <h2 style='color: #4f46e5; text-align: center;'>Welcome to TutaFlow</h2>
                    <p>Xin chào {req.FullName}, tài khoản của bạn đã được tạo thành công bởi Quản trị viên.</p>
                    <p>Vui lòng đăng nhập với thông tin sau:</p>
                    <ul>
                        <li><b>Email:</b> {emailLower}</li>
                        <li><b>Mật khẩu tạm thời:</b> <span style='background-color: #f1f5f9; padding: 2px 6px; border-radius: 4px; font-family: monospace;'>{tempPassword}</span></li>
                    </ul>
                    <p>Bạn sẽ được yêu cầu đổi mật khẩu ngay trong lần đăng nhập đầu tiên.</p>
                </div>";
            emailQueue.QueueEmail(emailLower, subject, body);

            return Ok(new ApiResponse<object>(true, "Tạo nhân viên thành công.", null));
        }

        [HttpPost("{id}/reset-temporary-password")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ResetTemporaryPassword(Guid id, [FromServices] IEmailQueue emailQueue)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<object>(false, "Không tìm thấy người dùng.", null));
            }

            var tempPassword = PasswordHelper.GenerateRandomPassword(10);
            user.PasswordHash = PasswordHelper.HashPassword(tempPassword);
            user.RequiresPasswordChange = true;
            await _userRepo.SaveAsync();

            var subject = "TutaFlow - Mật khẩu tạm thời mới";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                    <h2 style='color: #4f46e5; text-align: center;'>TutaFlow Security</h2>
                    <p>Quản trị viên vừa tạo lại mật khẩu tạm thời cho tài khoản của bạn.</p>
                    <p>Vui lòng đăng nhập với thông tin sau:</p>
                    <ul>
                        <li><b>Email:</b> {user.Email}</li>
                        <li><b>Mật khẩu tạm thời mới:</b> <span style='background-color: #f1f5f9; padding: 2px 6px; border-radius: 4px; font-family: monospace;'>{tempPassword}</span></li>
                    </ul>
                    <p>Bạn sẽ được yêu cầu đổi mật khẩu ngay trong lần đăng nhập tới.</p>
                </div>";
            emailQueue.QueueEmail(user.Email, subject, body);

            return Ok(new ApiResponse<object>(true, "Đã gửi lại mật khẩu tạm thời mới vào email nhân viên.", null));
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest req)
        {
            if (string.IsNullOrEmpty(req.Role) || (req.Role != "Manager" && req.Role != "Member"))
            {
                return BadRequest(new ApiResponse<object>(false, "Role không hợp lệ. Chỉ chấp nhận Manager hoặc Member.", null));
            }
            
            var result = await _userRepo.UpdateUserRoleAsync(id, req.Role);
            if (!result) return NotFound(new ApiResponse<object>(false, "Không tìm thấy người dùng.", null));

            return Ok(new ApiResponse<object>(true, "Cập nhật quyền thành công.", null));
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest req)
        {
            var result = await _userRepo.UpdateUserStatusAsync(id, req.IsActive);
            if (!result) return NotFound(new ApiResponse<object>(false, "Không tìm thấy người dùng.", null));

            return Ok(new ApiResponse<object>(true, "Cập nhật trạng thái thành công.", null));
        }
    }
    
    public class UpdateRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
