using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _dbContext;

        public ProjectRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Project?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Projects
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public Project Add(Project project)
        {
            return _dbContext.Projects.Add(project).Entity;
        }

        public void Delete(Project project)
        {
            _dbContext.Projects.Remove(project);
        }

        public async Task<List<Project>> GetProjectsByUserIdAsync(Guid userId)
        {
            return await _dbContext.Projects
                .AsNoTracking()
                .Include(p => p.Owner)
                .Where(p => p.ProjectMembers.Any(pm => pm.UserId == userId))
                .ToListAsync();
        }

        public async Task<List<ProjectMember>> GetProjectMembersAsync(Guid projectId)
        {
            return await _dbContext.ProjectMembers
                .AsNoTracking()
                .Where(pm => pm.ProjectId == projectId)
                .Include(pm => pm.User)
                .ToListAsync();
        }

        public async Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId)
        {
            return await _dbContext.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public void AddMember(ProjectMember member)
        {
            _dbContext.ProjectMembers.Add(member);
        }

        public void RemoveMember(ProjectMember member)
        {
            _dbContext.ProjectMembers.Remove(member);
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ProjectMember>> GetProjectMembersWithProjectsByUserIdAsync(Guid userId)
        {
            return await _dbContext.ProjectMembers
                .AsNoTracking()
                .Where(pm => pm.UserId == userId)
                .Include(pm => pm.Project)
                    .ThenInclude(p => p.Owner)
                .ToListAsync();
        }
    }
}
