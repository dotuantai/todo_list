using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;

namespace API_v2.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId:int}/feed")]
    [Authorize]
    public class TaskFeedController : BaseApiController
    {
        private readonly ITaskFeedService _feedService;

        public TaskFeedController(ITaskFeedService feedService)
        {
            _feedService = feedService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTaskFeed(int taskId, [FromQuery] int page = 1, [FromQuery] int pageSize = 15)
        {
            var result = await _feedService.GetTaskFeedAsync(taskId, CurrentUserId, page, pageSize);
            return Ok(new ApiResponse<PagedResponse<TaskFeedItemDto>>(true, "Feed retrieved successfully.", result));
        }
    }
}
