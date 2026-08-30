using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Exceptions;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Models.Constants;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class ProjectColumnService : IProjectColumnService
    {
        private readonly IProjectColumnRepository _columnRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly INotificationService _notificationService;

        public ProjectColumnService(
            IProjectColumnRepository columnRepo,
            IProjectRepository projectRepo,
            INotificationService notificationService)
        {
            _columnRepo = columnRepo;
            _projectRepo = projectRepo;
            _notificationService = notificationService;
        }

        public async Task<List<ProjectColumnResponse>> GetColumnsAsync(Guid projectId, Guid userId)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, userId);
            if (member is null && !await _projectRepo.IsSystemAdminAsync(userId))
            {
                throw ApiException.Forbidden("You are not a member of this project.");
            }

            var columns = await _columnRepo.GetColumnsByProjectIdAsync(projectId);
            return columns.Select(MapToResponse).ToList();
        }

        public async Task<ProjectColumnResponse> CreateColumnAsync(Guid projectId, CreateProjectColumnRequest req, Guid currentUserId)
        {
            await VerifyOwnerOrManagerAsync(projectId, currentUserId, "Only Owners or Managers can create columns.");

            var column = new ProjectColumn
            {
                ProjectId = projectId, Name = req.Name.Trim(), IsCompletedStage = req.IsCompletedStage,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _columnRepo.CreateAtOrderAsync(column, req.Order);
            await _notificationService.SendColumnsReorderedAsync(projectId, result.Columns.Select(MapToResponse).ToList());
            return MapToResponse(result.Column);
        }

        public async Task<ProjectColumnResponse> UpdateColumnAsync(int columnId, UpdateProjectColumnRequest req, Guid currentUserId)
        {
            var column = await _columnRepo.GetByIdAsync(columnId);
            if (column is null)
                throw ApiException.NotFound("Column not found.");

            await VerifyOwnerOrManagerAsync(column.ProjectId, currentUserId, "Only Owners or Managers can update columns.");

            var projectId = column.ProjectId;
            var result = await _columnRepo.UpdateAndReorderAsync(columnId, req.Name.Trim(), req.Order, req.IsCompletedStage);
            await _notificationService.SendColumnsReorderedAsync(projectId, result.Columns.Select(MapToResponse).ToList());
            return MapToResponse(result.Column);
        }

        public async Task<string> DeleteColumnAsync(int columnId, Guid currentUserId)
        {
            var column = await _columnRepo.GetByIdAsync(columnId);
            if (column is null)
                throw ApiException.NotFound("Column not found.");

            await VerifyOwnerOrManagerAsync(column.ProjectId, currentUserId, "Only Owners or Managers can delete columns.");

            if (column.Tasks.Any())
            {
                throw ApiException.BadRequest("Cannot delete a column that contains tasks. Please move the tasks first.");
            }

            _columnRepo.Delete(column);
            await _columnRepo.SaveAsync();

            return "Column deleted successfully.";
        }

        private ProjectColumnResponse MapToResponse(ProjectColumn column)
        {
            return new ProjectColumnResponse
            {
                Id = column.Id,
                Name = column.Name,
                Order = column.Order,
                IsCompletedStage = column.IsCompletedStage
            };
        }

        private async Task VerifyOwnerOrManagerAsync(Guid projectId, Guid userId, string errorMessage)
        {
            if (await _projectRepo.IsSystemAdminAsync(userId))
            {
                return;
            }

            var member = await _projectRepo.GetMemberAsync(projectId, userId);
            if (member is null || !ProjectRoles.IsOwnerOrManager(member.Role))
            {
                throw ApiException.Forbidden(errorMessage);
            }
        }
    }
}
