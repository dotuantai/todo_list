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

        public ProjectColumnService(IProjectColumnRepository columnRepo, IProjectRepository projectRepo)
        {
            _columnRepo = columnRepo;
            _projectRepo = projectRepo;
        }

        public async Task<List<ProjectColumnResponse>> GetColumnsAsync(Guid projectId, Guid userId)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, userId);
            if (member is null)
            {
                throw ApiException.Forbidden("You are not a member of this project.");
            }

            var columns = await _columnRepo.GetColumnsByProjectIdAsync(projectId);
            return columns.Select(MapToResponse).ToList();
        }

        public async Task<ProjectColumnResponse> CreateColumnAsync(Guid projectId, CreateProjectColumnRequest req, Guid currentUserId)
        {
            await VerifyOwnerOrManagerAsync(projectId, currentUserId, "Only Owners or Managers can create columns.");

            var existingColumns = await _columnRepo.GetColumnsByProjectIdAsync(projectId);
            var columnsToShift = existingColumns.Where(c => c.Order >= req.Order).ToList();
            foreach (var col in columnsToShift)
            {
                col.Order += 1;
            }

            var column = new ProjectColumn
            {
                ProjectId = projectId,
                Name = req.Name.Trim(),
                Order = req.Order,
                IsCompletedStage = req.IsCompletedStage,
                CreatedAt = DateTime.UtcNow
            };

            _columnRepo.Add(column);
            await _columnRepo.SaveAsync();

            return MapToResponse(column);
        }

        public async Task<ProjectColumnResponse> UpdateColumnAsync(int columnId, UpdateProjectColumnRequest req, Guid currentUserId)
        {
            var column = await _columnRepo.GetByIdAsync(columnId);
            if (column is null)
                throw ApiException.NotFound("Column not found.");

            await VerifyOwnerOrManagerAsync(column.ProjectId, currentUserId, "Only Owners or Managers can update columns.");

            var existingColumns = await _columnRepo.GetColumnsByProjectIdAsync(column.ProjectId);
            
            int oldOrder = column.Order;
            int newOrder = req.Order;

            if (oldOrder != newOrder)
            {
                if (oldOrder < newOrder)
                {
                    foreach (var c in existingColumns.Where(x => x.Order > oldOrder && x.Order <= newOrder && x.Id != columnId))
                    {
                        c.Order -= 1;
                    }
                }
                else
                {
                    foreach (var c in existingColumns.Where(x => x.Order >= newOrder && x.Order < oldOrder && x.Id != columnId))
                    {
                        c.Order += 1;
                    }
                }
            }

            column.Name = req.Name.Trim();
            column.Order = req.Order;
            column.IsCompletedStage = req.IsCompletedStage;

            await _columnRepo.SaveAsync();
            return MapToResponse(column);
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
            var member = await _projectRepo.GetMemberAsync(projectId, userId);
            if (member is null || !ProjectRoles.IsOwnerOrManager(member.Role))
            {
                throw ApiException.Forbidden(errorMessage);
            }
        }
    }
}
