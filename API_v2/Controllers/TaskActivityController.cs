using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_v2.Datas;
using API_v2.Models;
using API_v2.Models.DTOs;
using System.Security.Claims;
using System.Text.Json;

namespace API_v2.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId:int}/activities")]
    [Authorize]
    public class TaskActivityController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TaskActivityController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET /api/tasks/{taskId}/activities
        /// Returns all activity records for a task, ordered by ChangedAt ascending.
        /// Requires membership in the task's project.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetActivities(int taskId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid currentUserId))
                return Unauthorized(new { Success = false, Message = "Unauthorized." });

            var task = await _db.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return NotFound(new { Success = false, Message = $"Task #{taskId} not found." });

            // Verify membership if task belongs to a project
            if (task.ProjectId.HasValue)
            {
                var isMember = await _db.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == task.ProjectId.Value && pm.UserId == currentUserId);
                if (!isMember)
                    return Forbid();
            }

            var activities = await _db.TaskActivities
                .Where(a => a.TaskId == taskId)
                .Include(a => a.User)
                .OrderBy(a => a.ChangedAt)
                .ToListAsync();

            var result = activities.Select(a =>
            {
                List<FieldChangeDto> changes;
                try
                {
                    changes = JsonSerializer.Deserialize<List<FieldChangeDto>>(a.Changes,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<FieldChangeDto>();
                }
                catch
                {
                    changes = new List<FieldChangeDto>();
                }

                return new TaskActivityResponse
                {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    UserId = a.UserId,
                    UserEmail = a.User?.Email ?? "Unknown",
                    ChangedAt = a.ChangedAt,
                    Changes = changes
                };
            }).ToList();

            return Ok(new { Success = true, Message = "Activities retrieved.", Data = result });
        }
    }
}
