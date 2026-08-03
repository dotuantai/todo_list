using System;
using System.Security.Claims;
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
    public class TaskCommentController : ControllerBase
    {
        private readonly ITaskCommentService _commentService;

        public TaskCommentController(ITaskCommentService commentService)
        {
            _commentService = commentService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                throw new UnauthorizedAccessException("User not found in token.");
            }
            return Guid.Parse(userIdStr);
        }

        [HttpGet("{taskId}/comments")]
        public async Task<IActionResult> GetComments(int taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var comments = await _commentService.GetCommentsAsync(taskId, userId);
                return Ok(new ApiResponse<object>(true, "Comments retrieved successfully.", comments));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>(false, ex.Message, null));
            }
        }

        [HttpPost("{taskId}/comments")]
        public async Task<IActionResult> CreateComment(int taskId, [FromBody] CreateTaskCommentRequest req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid data.", null));
            }

            try
            {
                var userId = GetCurrentUserId();
                var comment = await _commentService.CreateCommentAsync(taskId, req, userId);
                return Ok(new ApiResponse<object>(true, "Comment added successfully.", comment));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>(false, ex.Message, null));
            }
        }

        [HttpPut("comments/{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateTaskCommentRequest req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid data.", null));
            }

            try
            {
                var userId = GetCurrentUserId();
                var comment = await _commentService.UpdateCommentAsync(id, req, userId);
                return Ok(new ApiResponse<object>(true, "Comment updated successfully.", comment));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>(false, ex.Message, null));
            }
        }

        [HttpDelete("comments/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _commentService.DeleteCommentAsync(id, userId);
                return Ok(new ApiResponse<object>(true, "Comment deleted successfully.", null));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>(false, ex.Message, null));
            }
        }
    }
}
