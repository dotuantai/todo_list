using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;

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
