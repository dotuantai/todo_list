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
        public ActionResult GetMyProjects()
        {
            var result = _projectService.GetProjectsForUser(CurrentUserId);
            return Ok(new ApiResponse<List<ProjectResponse>>(true, "Success", result));
        }

        [HttpPost]
        public ActionResult Create([FromBody] CreateProjectRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid project data.", null));
            }

            var result = _projectService.CreateProject(req, CurrentUserId);
            return Ok(new ApiResponse<ProjectResponse>(true, "Project created successfully.", result));
        }

        [HttpGet("{projectId:guid}")]
        public ActionResult GetProjectDetail(Guid projectId)
        {
            var result = _projectService.GetProjectDetail(projectId, CurrentUserId);
            return Ok(new ApiResponse<ProjectResponse>(true, "Success", result));
        }

        [HttpPut("{projectId:guid}")]
        public ActionResult UpdateProject(Guid projectId, [FromBody] UpdateProjectRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid project data.", null));
            }

            var result = _projectService.UpdateProject(projectId, req, CurrentUserId);
            return Ok(new ApiResponse<ProjectResponse>(true, "Project updated successfully.", result));
        }

        [HttpDelete("{projectId:guid}")]
        public ActionResult DeleteProject(Guid projectId)
        {
            _projectService.DeleteProject(projectId, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Project deleted successfully.", null));
        }

        [HttpGet("{projectId:guid}/members")]
        public ActionResult GetMembers(Guid projectId)
        {
            var result = _projectService.GetMembers(projectId);
            return Ok(new ApiResponse<List<MemberResponse>>(true, "Success", result));
        }

        [HttpPost("{projectId:guid}/members")]
        public ActionResult AddMember(Guid projectId, [FromBody] AddMemberRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid member data.", null));
            }

            var result = _projectService.AddMember(projectId, req, CurrentUserId);
            return Ok(new ApiResponse<MemberResponse>(true, "Member added successfully.", result));
        }

        [HttpPut("{projectId:guid}/members/{userId:guid}")]
        public ActionResult UpdateMemberRole(Guid projectId, Guid userId, [FromBody] UpdateMemberRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid member data.", null));
            }

            var result = _projectService.UpdateMemberRole(projectId, userId, req, CurrentUserId);
            return Ok(new ApiResponse<MemberResponse>(true, "Member role updated successfully.", result));
        }

        [HttpDelete("{projectId:guid}/members/{userId:guid}")]
        public ActionResult RemoveMember(Guid projectId, Guid userId)
        {
            _projectService.RemoveMember(projectId, userId, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Member removed successfully.", null));
        }

        [HttpGet("{projectId:guid}/tasks")]
        public ActionResult GetProjectTasks(Guid projectId)
        {
            var result = _taskService.GetProjectTasks(projectId, CurrentUserId);
            return Ok(new ApiResponse<List<TaskDetailResponse>>(true, "Success", result));
        }

        [HttpPost("{projectId:guid}/tasks")]
        public ActionResult CreateTask(Guid projectId, [FromBody] CreateTaskRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid task data.", null));
            }

            var result = _taskService.CreateTask(req, CurrentUserId, projectId);
            return Ok(new ApiResponse<string>(true, result, null));
        }
    }
}
