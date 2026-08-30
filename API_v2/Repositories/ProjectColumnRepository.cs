using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_v2.Repositories
{
    public class ProjectColumnRepository : IProjectColumnRepository
    {
        private readonly AppDbContext _db;

        public ProjectColumnRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ProjectColumn>> GetColumnsByProjectIdAsync(Guid projectId)
        {
            return await _db.ProjectColumns
                .AsNoTracking()
                .Where(c => c.ProjectId == projectId)
                .OrderBy(c => c.Order)
                .ToListAsync();
        }

        public async Task<ProjectColumn?> GetByIdAsync(int id)
        {
            return await _db.ProjectColumns
                .Include(c => c.Tasks)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<(ProjectColumn Column, List<ProjectColumn> Columns)> CreateAtOrderAsync(ProjectColumn column, int requestedOrder)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await AcquireProjectLockAsync(column.ProjectId);
                var columns = await GetTrackedForUpdateAsync(column.ProjectId);
                var newOrder = Math.Clamp(requestedOrder, 0, columns.Count);
                await MoveToTemporaryRangeAsync(columns);
                columns.Insert(newOrder, column);
                for (var index = 0; index < columns.Count; index++) columns[index].Order = index;
                _db.ProjectColumns.Add(column);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return (column, columns);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(ProjectColumn Column, List<ProjectColumn> Columns)> UpdateAndReorderAsync(
            int columnId, string name, int requestedOrder, bool isCompletedStage)
        {
            var projectId = await _db.ProjectColumns.AsNoTracking()
                .Where(column => column.Id == columnId).Select(column => (Guid?)column.ProjectId).FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Column not found.");
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.ChangeTracker.Clear();
                await AcquireProjectLockAsync(projectId);
                var columns = await GetTrackedForUpdateAsync(projectId);
                var column = columns.SingleOrDefault(item => item.Id == columnId)
                    ?? throw new InvalidOperationException("Column not found.");
                var newOrder = Math.Clamp(requestedOrder, 0, columns.Count - 1);
                await MoveToTemporaryRangeAsync(columns);
                columns.Remove(column);
                columns.Insert(newOrder, column);
                for (var index = 0; index < columns.Count; index++) columns[index].Order = index;
                column.Name = name;
                column.IsCompletedStage = isCompletedStage;
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return (column, columns);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private Task AcquireProjectLockAsync(Guid projectId) => _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({projectId.ToString()}, 0))");

        private Task<List<ProjectColumn>> GetTrackedForUpdateAsync(Guid projectId) => _db.ProjectColumns
            .FromSqlInterpolated($"SELECT * FROM \"ProjectColumns\" WHERE \"ProjectId\" = {projectId} FOR UPDATE")
            .OrderBy(column => column.Order).ToListAsync();

        private async Task MoveToTemporaryRangeAsync(IReadOnlyList<ProjectColumn> columns)
        {
            for (var index = 0; index < columns.Count; index++) columns[index].Order = -(index + 1);
            await _db.SaveChangesAsync();
        }

        public void Add(ProjectColumn column)
        {
            _db.ProjectColumns.Add(column);
        }

        public void Delete(ProjectColumn column)
        {
            _db.ProjectColumns.Remove(column);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
