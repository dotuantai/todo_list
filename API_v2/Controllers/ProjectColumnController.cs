using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;

namespace API_v2.Controllers
{
    [Route("api/projects/{projectId:guid}/columns")]
    [Authorize]
    public class ProjectColumnController : BaseApiController
    {
        private readonly IProjectColumnService _columnService;

        public ProjectColumnController(IProjectColumnService columnService)
        {
            _columnService = columnService;
        }

        [HttpGet]
        public async Task<ActionResult> GetColumns(Guid projectId)
        {
            var result = await _columnService.GetColumnsAsync(projectId, CurrentUserId);
            return Ok(new ApiResponse<List<ProjectColumnResponse>>(true, "Success", result));
        }

        [HttpPost]
        public async Task<ActionResult> CreateColumn(Guid projectId, [FromBody] CreateProjectColumnRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid column data.", null));
            }

            var result = await _columnService.CreateColumnAsync(projectId, req, CurrentUserId);
            return Ok(new ApiResponse<ProjectColumnResponse>(true, "Column created successfully.", result));
        }

        [HttpPut("{columnId:int}")]
        public async Task<ActionResult> UpdateColumn(Guid projectId, int columnId, [FromBody] UpdateProjectColumnRequest req)
        {
            if (req is null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid column data.", null));
            }

            var result = await _columnService.UpdateColumnAsync(columnId, req, CurrentUserId);
            return Ok(new ApiResponse<ProjectColumnResponse>(true, "Column updated successfully.", result));
        }

        [HttpDelete("{columnId:int}")]
        public async Task<ActionResult> DeleteColumn(Guid projectId, int columnId)
        {
            await _columnService.DeleteColumnAsync(columnId, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Column deleted successfully.", null));
        }
    }
}
