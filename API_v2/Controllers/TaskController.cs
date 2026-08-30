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
        private readonly ITaskImportExportService _taskImportExportService;

        public TaskController(ITaskService taskService, ITaskImportExportService taskImportExportService)
        {
            _taskService = taskService;
            _taskImportExportService = taskImportExportService;
        }

        [HttpGet("~/api/projects/{projectId:guid}/tasks")]
        public async Task<ActionResult> GetProjectTasks(Guid projectId, [FromQuery] int? columnId,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null,
            [FromQuery] API_v2.Models.Enums.TaskPriority? priority = null, [FromQuery] Guid? assigneeId = null)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var result = await _taskService.GetProjectTasksAsync(
                projectId, CurrentUserId, columnId, page, pageSize, search, priority, assigneeId);
            return Ok(new ApiResponse<PagedResponse<TaskDetailResponse>>(true, "Success", result));
        }

        [HttpGet("~/api/projects/{projectId:guid}/tasks/stats")]
        public async Task<ActionResult> GetTaskStats(Guid projectId)
        {
            var result = await _taskService.GetTaskStatsAsync(projectId, CurrentUserId);
            return Ok(new ApiResponse<TaskStatsResponse>(true, "Success", result));
        }

        [HttpPost("~/api/projects/{projectId:guid}/tasks")]
        public async Task<ActionResult> CreateTask(Guid projectId, [FromBody] CreateTaskRequest req)
        {
            var result = await _taskService.CreateTaskAsync(req, CurrentUserId, projectId);
            return Ok(new ApiResponse<string>(true, result, null));
        }

        [HttpGet("~/api/projects/tasks/template")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTaskTemplate()
        {
            var fileBytes = await _taskImportExportService.GetTaskTemplateAsync();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TaskTemplate.xlsx");
        }

        [HttpPost("~/api/projects/{projectId:guid}/tasks/import")]
        public async Task<ActionResult> ImportTasks(Guid projectId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse<object>(false, "No file uploaded.", null));
            await using var stream = file.OpenReadStream();
            var count = await _taskImportExportService.ImportTasksAsync(projectId, CurrentUserId, stream, file.FileName);
            return Ok(new ApiResponse<int>(true, $"{count} tasks imported successfully.", count));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateTaskRequest req)
        {
            var result = await _taskService.UpdateTaskAsync(id, req, CurrentUserId);
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
            var result = await _taskService.AssignTaskAsync(req, CurrentUserId);
            return Ok(new ApiResponse<string>(true, result, null));
        }


        [HttpDelete("{taskId}/assignments/{userId}")]
        public async Task<ActionResult> RemoveAssignment(int taskId, Guid userId)
        {
            var result = await _taskService.RemoveAssignmentAsync(taskId, userId, CurrentUserId);
            return Ok(new ApiResponse<string>(true, result, null));
        }

        [HttpPut("status")]
        public async Task<ActionResult> ChangeColumn([FromBody] ChangeTaskColumnRequest req)
        {
            await _taskService.ChangeColumnAsync(req, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Task column updated successfully.", null));
        }
    }
}
