using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;

namespace API_v2.Controllers
{
    [Route("api/projects")]
    [Authorize]
    public class ProjectController : BaseApiController
    {
        private readonly IProjectService _projectService;
        private readonly ITaskService _taskService;

        public ProjectController(IProjectService projectService, ITaskService taskService)
        {
            _projectService = projectService;
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult> GetMyProjects()
        {
            var result = await _projectService.GetProjectsForUserAsync(CurrentUserId);
            return Ok(new ApiResponse<List<ProjectResponse>>(true, "Success", result));
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateProjectRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid project data.", null));
            }

            var result = await _projectService.CreateProjectAsync(req, CurrentUserId);
            return Ok(new ApiResponse<ProjectResponse>(true, "Project created successfully.", result));
        }

        [HttpGet("{projectId:guid}")]
        public async Task<ActionResult> GetProjectDetail(Guid projectId)
        {
            var result = await _projectService.GetProjectDetailAsync(projectId, CurrentUserId);
            return Ok(new ApiResponse<ProjectResponse>(true, "Success", result));
        }

        [HttpPut("{projectId:guid}")]
        public async Task<ActionResult> UpdateProject(Guid projectId, [FromBody] UpdateProjectRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid project data.", null));
            }

            var result = await _projectService.UpdateProjectAsync(projectId, req, CurrentUserId);
            return Ok(new ApiResponse<ProjectResponse>(true, "Project updated successfully.", result));
        }

        [HttpDelete("{projectId:guid}")]
        public async Task<ActionResult> DeleteProject(Guid projectId)
        {
            await _projectService.DeleteProjectAsync(projectId, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Project deleted successfully.", null));
        }

        [HttpGet("{projectId:guid}/members")]
        public async Task<ActionResult> GetMembers(Guid projectId)
        {
            var result = await _projectService.GetMembersAsync(projectId, CurrentUserId);
            return Ok(new ApiResponse<List<MemberResponse>>(true, "Success", result));
        }

        [HttpPost("{projectId:guid}/members")]
        public async Task<ActionResult> AddMember(Guid projectId, [FromBody] AddMemberRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid member data.", null));
            }

            var result = await _projectService.AddMemberAsync(projectId, req, CurrentUserId);
            return Ok(new ApiResponse<MemberResponse>(true, "Member added successfully.", result));
        }

        [HttpPut("{projectId:guid}/members/{userId:guid}")]
        public async Task<ActionResult> UpdateMemberRole(Guid projectId, Guid userId, [FromBody] UpdateMemberRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid member data.", null));
            }

            var result = await _projectService.UpdateMemberRoleAsync(projectId, userId, req, CurrentUserId);
            return Ok(new ApiResponse<MemberResponse>(true, "Member role updated successfully.", result));
        }

        [HttpDelete("{projectId:guid}/members/{userId:guid}")]
        public async Task<ActionResult> RemoveMember(Guid projectId, Guid userId)
        {
            await _projectService.RemoveMemberAsync(projectId, userId, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Member removed successfully.", null));
        }

        [HttpGet("{projectId:guid}/tasks")]
        public async Task<ActionResult> GetProjectTasks(Guid projectId, [FromQuery] int? columnId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string search = null, [FromQuery] API_v2.Models.Enums.TaskPriority? priority = null, [FromQuery] Guid? assigneeId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var result = await _taskService.GetProjectTasksAsync(projectId, CurrentUserId, columnId, page, pageSize, search, priority, assigneeId);
            return Ok(new ApiResponse<PagedResponse<TaskDetailResponse>>(true, "Success", result));
        }

        [HttpGet("{projectId:guid}/tasks/stats")]
        public async Task<ActionResult> GetTaskStats(Guid projectId)
        {
            var result = await _taskService.GetTaskStatsAsync(projectId, CurrentUserId);
            return Ok(new ApiResponse<TaskStatsResponse>(true, "Success", result));
        }

        [HttpPost("{projectId:guid}/tasks")]
        public async Task<ActionResult> CreateTask(Guid projectId, [FromBody] CreateTaskRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid task data.", null));
            }

            var result = await _taskService.CreateTaskAsync(req, CurrentUserId, projectId);
            return Ok(new ApiResponse<string>(true, result, null));
        }
    }
}
