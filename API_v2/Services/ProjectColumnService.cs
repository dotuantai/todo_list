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
using API_v2.Datas;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Services
{
    public class ProjectColumnService : IProjectColumnService
    {
        private readonly IProjectColumnRepository _columnRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly AppDbContext _db;
        private readonly INotificationService _notificationService;

        public ProjectColumnService(
            IProjectColumnRepository columnRepo,
            IProjectRepository projectRepo,
            AppDbContext db,
            INotificationService notificationService)
        {
            _columnRepo = columnRepo;
            _projectRepo = projectRepo;
            _db = db;
            _notificationService = notificationService;
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

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await AcquireProjectColumnLockAsync(projectId);
                var existingColumns = await GetTrackedColumnsForUpdateAsync(projectId);
                var newOrder = Math.Clamp(req.Order, 0, existingColumns.Count);

                await MoveOrdersToTemporaryRangeAsync(existingColumns);

                var column = new ProjectColumn
                {
                    ProjectId = projectId,
                    Name = req.Name.Trim(),
                    Order = newOrder,
                    IsCompletedStage = req.IsCompletedStage,
                    CreatedAt = DateTime.UtcNow
                };

                existingColumns.Insert(newOrder, column);
                for (var index = 0; index < existingColumns.Count; index++)
                {
                    existingColumns[index].Order = index;
                }

                _columnRepo.Add(column);
                await _columnRepo.SaveAsync();
                await transaction.CommitAsync();

                await _notificationService.SendColumnsReorderedAsync(
                    projectId,
                    existingColumns.Select(MapToResponse).ToList());
                return MapToResponse(column);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProjectColumnResponse> UpdateColumnAsync(int columnId, UpdateProjectColumnRequest req, Guid currentUserId)
        {
            var column = await _columnRepo.GetByIdAsync(columnId);
            if (column is null)
                throw ApiException.NotFound("Column not found.");

            await VerifyOwnerOrManagerAsync(column.ProjectId, currentUserId, "Only Owners or Managers can update columns.");

            var projectId = column.ProjectId;
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // The preliminary authorization read may be tracked; reload the
                // authoritative ordered set only after taking the project lock.
                _db.ChangeTracker.Clear();
                await AcquireProjectColumnLockAsync(projectId);
                var existingColumns = await GetTrackedColumnsForUpdateAsync(projectId);
                column = existingColumns.SingleOrDefault(c => c.Id == columnId)
                    ?? throw ApiException.NotFound("Column not found.");

                var newOrder = Math.Clamp(req.Order, 0, existingColumns.Count - 1);
                await MoveOrdersToTemporaryRangeAsync(existingColumns);

                existingColumns.Remove(column);
                existingColumns.Insert(newOrder, column);
                for (var index = 0; index < existingColumns.Count; index++)
                {
                    existingColumns[index].Order = index;
                }

                column.Name = req.Name.Trim();
                column.IsCompletedStage = req.IsCompletedStage;
                await _columnRepo.SaveAsync();
                await transaction.CommitAsync();

                var responses = existingColumns.Select(MapToResponse).ToList();
                await _notificationService.SendColumnsReorderedAsync(projectId, responses);
                return MapToResponse(column);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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

        private async Task AcquireProjectColumnLockAsync(Guid projectId)
        {
            // A transaction-scoped PostgreSQL advisory lock also serializes the
            // first two column creations, when there are no rows to FOR UPDATE.
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({projectId.ToString()}, 0))");
        }

        private async Task<List<ProjectColumn>> GetTrackedColumnsForUpdateAsync(Guid projectId)
        {
            return await _db.ProjectColumns
                .FromSqlInterpolated($"SELECT * FROM \"ProjectColumns\" WHERE \"ProjectId\" = {projectId} FOR UPDATE")
                .OrderBy(c => c.Order)
                .ToListAsync();
        }

        private async Task MoveOrdersToTemporaryRangeAsync(IReadOnlyList<ProjectColumn> columns)
        {
            for (var index = 0; index < columns.Count; index++)
            {
                columns[index].Order = -(index + 1);
            }

            await _columnRepo.SaveAsync();
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
