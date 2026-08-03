using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_v2.Datas;
using API_v2.Models.DTOs;
using System.Security.Claims;
using System.Text.Json;

namespace API_v2.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId:int}/feed")]
    [Authorize]
    public class TaskFeedController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TaskFeedController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetTaskFeed(int taskId, [FromQuery] int page = 1, [FromQuery] int pageSize = 15)
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
                var member = await _db.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == task.ProjectId.Value && pm.UserId == currentUserId);
                
                if (member == null)
                {
                    return StatusCode(403, new { Success = false, Message = "You do not have access to this project." });
                }
            }

            var comments = await _db.TaskComments
                .Include(c => c.User)
                .Where(c => c.TaskId == taskId)
                .ToListAsync();

            var activities = await _db.TaskActivities
                .Include(a => a.User)
                .Where(a => a.TaskId == taskId)
                .ToListAsync();

            var feed = new List<TaskFeedItemDto>();

            foreach (var c in comments)
            {
                feed.Add(new TaskFeedItemDto
                {
                    Type = "comment",
                    Id = c.Id,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId,
                    UserName = c.User?.Email ?? "Unknown",
                    Content = c.Content
                });
            }

            foreach (var a in activities)
            {
                var changes = new List<FieldChangeDto>();
                if (!string.IsNullOrEmpty(a.Changes))
                {
                    try
                    {
                        changes = JsonSerializer.Deserialize<List<FieldChangeDto>>(a.Changes) ?? new List<FieldChangeDto>();
                    }
                    catch { }
                }

                feed.Add(new TaskFeedItemDto
                {
                    Type = "activity",
                    Id = a.Id,
                    CreatedAt = a.ChangedAt,
                    UserId = a.UserId,
                    UserName = a.User?.Email ?? "Unknown",
                    Changes = changes
                });
            }

            // Sort descending (newest first)
            feed = feed.OrderByDescending(f => f.CreatedAt).ToList();

            var totalCount = feed.Count;
            var pagedItems = feed.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            // Reverse so they are returned chronologically ascending (oldest first)
            pagedItems.Reverse();

            return Ok(new
            {
                Success = true,
                Message = "Feed retrieved successfully.",
                Data = new PagedResponse<TaskFeedItemDto>
                {
                    Items = pagedItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                }
            });
        }
    }
}
