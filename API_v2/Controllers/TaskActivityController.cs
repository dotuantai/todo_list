using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;

namespace API_v2.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId:int}/activities")]
    [Authorize]
    public class TaskActivityController : BaseApiController
    {
        private readonly ITaskActivityService _activityService;

        public TaskActivityController(ITaskActivityService activityService)
        {
            _activityService = activityService;
        }

        /// <summary>
        /// GET /api/tasks/{taskId}/activities
        /// Returns all activity records for a task, ordered by ChangedAt ascending.
        /// Requires membership in the task's project.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetActivities(int taskId)
        {
            var result = await _activityService.GetActivitiesAsync(taskId, CurrentUserId);
            return Ok(new ApiResponse<List<TaskActivityResponse>>(true, "Activities retrieved.", result));
        }
    }
}
