using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;

namespace API_v2.Controllers
{
    [Route("api/tasks")]
    [Authorize]
    public class TaskController : BaseApiController
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] UpdateTaskRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid task data.", null));
            }

            var result = await _taskService.UpdateTaskAsync(req, CurrentUserId);
            return Ok(new ApiResponse<string>(true, result, null));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id, CurrentUserId);
            return Ok(new ApiResponse<string>(true, result, null));
        }

        [HttpPost("assign")]
        public async Task<ActionResult> Assign([FromBody] AssignTaskRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid assignment data.", null));
            }

            var result = await _taskService.AssignTaskAsync(req, CurrentUserId);
            return Ok(new ApiResponse<string>(true, result, null));
        }


        [HttpDelete("assign")]
        public async Task<ActionResult> RemoveAssignment([FromBody] RemoveAssignmentRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid assignment data.", null));
            }

            var result = await _taskService.RemoveAssignmentAsync(req, CurrentUserId);
            return Ok(new ApiResponse<string>(true, result, null));
        }

        [HttpPut("status")]
        public async Task<ActionResult> ChangeStatus([FromBody] ChangeTaskStatusRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid status data.", null));
            }

            await _taskService.ChangeStatusAsync(req, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Task status updated successfully.", null));
        }
    }
}
