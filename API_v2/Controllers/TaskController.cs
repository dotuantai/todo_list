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
        public ActionResult Update([FromBody] UpdateTaskRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid task data.", null));
            }

            var result = _taskService.UpdateTask(req, CurrentUserId);
            return Ok(new ApiResponse<string>(true, result, null));
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var result = _taskService.DeleteTask(id, CurrentUserId);
            return Ok(new ApiResponse<string>(true, result, null));
        }

        [HttpPost("assign")]
        public ActionResult Assign([FromBody] AssignTaskRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid assignment data.", null));
            }

            var result = _taskService.AssignTask(req, CurrentUserId);
            return Ok(new ApiResponse<string>(true, result, null));
        }

        [HttpPut("assign")]
        public ActionResult UpdatePermission([FromBody] AssignTaskRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid assignment data.", null));
            }

            var result = _taskService.UpdatePermission(req, CurrentUserId);
            return Ok(new ApiResponse<string>(true, result, null));
        }

        [HttpDelete("assign")]
        public ActionResult RemoveAssignment([FromBody] RemoveAssignmentRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid assignment data.", null));
            }

            var result = _taskService.RemoveAssignment(req, CurrentUserId);
            return Ok(new ApiResponse<string>(true, result, null));
        }

        [HttpPut("status")]
        public ActionResult ChangeStatus([FromBody] ChangeTaskStatusRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid status data.", null));
            }

            _taskService.ChangeStatus(req, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Task status updated successfully.", null));
        }
    }
}
