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

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<ActionResult> GetMyProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var result = await _projectService.GetProjectsForUserAsync(CurrentUserId, page, pageSize);
            return Ok(new ApiResponse<PagedResponse<ProjectResponse>>(true, "Success", result));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> Create([FromBody] CreateProjectRequest req)
        {
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
            var result = await _projectService.AddMemberAsync(projectId, req, CurrentUserId);
            return Ok(new ApiResponse<MemberResponse>(true, "Member added successfully.", result));
        }

        [HttpPut("{projectId:guid}/members/{userId:guid}")]
        public async Task<ActionResult> UpdateMemberRole(Guid projectId, Guid userId, [FromBody] UpdateMemberRequest req)
        {
            var result = await _projectService.UpdateMemberRoleAsync(projectId, userId, req, CurrentUserId);
            return Ok(new ApiResponse<MemberResponse>(true, "Member role updated successfully.", result));
        }

        [HttpDelete("{projectId:guid}/members/{userId:guid}")]
        public async Task<ActionResult> RemoveMember(Guid projectId, Guid userId)
        {
            await _projectService.RemoveMemberAsync(projectId, userId, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Member removed successfully.", null));
        }

    }
}
