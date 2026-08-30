using System.Threading.Tasks;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_v2.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TaskCommentController : BaseApiController
    {
        private readonly ITaskCommentService _commentService;

        public TaskCommentController(ITaskCommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{taskId}/comments")]
        public async Task<IActionResult> GetComments(int taskId, [FromQuery] int page = 1, [FromQuery] int limit = 5)
        {
            var comments = await _commentService.GetCommentsAsync(taskId, CurrentUserId, page, limit);
            return Ok(new ApiResponse<object>(true, "Comments retrieved successfully.", comments));
        }

        [HttpPost("{taskId}/comments")]
        public async Task<IActionResult> CreateComment(int taskId, [FromBody] CreateTaskCommentRequest req)
        {
            var comment = await _commentService.CreateCommentAsync(taskId, req, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Comment added successfully.", comment));
        }

        [HttpPut("comments/{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateTaskCommentRequest req)
        {
            var comment = await _commentService.UpdateCommentAsync(id, req, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Comment updated successfully.", comment));
        }

        [HttpDelete("comments/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            await _commentService.DeleteCommentAsync(id, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Comment deleted successfully.", null));
        }
    }
}
