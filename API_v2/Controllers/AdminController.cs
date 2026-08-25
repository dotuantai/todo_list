using System.Threading.Tasks;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("dashboard-stats")]
        public async Task<ActionResult<AdminDashboardResponse>> GetDashboardStats()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return Ok(new ApiResponse<AdminDashboardResponse>(true, "Successfully retrieved dashboard statistics.", stats));
        }
    }
}
