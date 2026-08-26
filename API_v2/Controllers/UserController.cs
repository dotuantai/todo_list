using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService) => _userService = userService;

        [HttpGet]
        public async Task<ActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var users = await _userService.GetUsersAsync(page, pageSize);
            return Ok(new ApiResponse<PagedResponse<AdminUserResponse>>(true, "Users retrieved.", users));
        }

        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            await _userService.CreateUserAsync(request);
            return Ok(new ApiResponse<object>(true, "User created.", null));
        }

        [HttpPost("{id:guid}/reset-temporary-password")]
        public async Task<ActionResult> ResetTemporaryPassword(Guid id)
        {
            await _userService.ResetTemporaryPasswordAsync(id);
            return Ok(new ApiResponse<object>(true, "Temporary password reset.", null));
        }

        [HttpPut("{id:guid}/role")]
        public async Task<ActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
        {
            await _userService.UpdateUserRoleAsync(id, request.Role);
            return Ok(new ApiResponse<object>(true, "User role updated.", null));
        }

        [HttpPut("{id:guid}/status")]
        public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            await _userService.UpdateUserStatusAsync(id, request.IsActive);
            return Ok(new ApiResponse<object>(true, "User status updated.", null));
        }
    }
}
