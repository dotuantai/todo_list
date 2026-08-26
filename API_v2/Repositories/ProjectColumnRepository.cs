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
