using API_v2.Datas;
using API_v2.Models;
using API_v2.Models.Constants;
using API_v2.Models.DTOs;
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
            var member = await _dbContext.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
                
            var user = await _dbContext.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            bool isAdmin = user?.Role?.Name == "Admin";

            if (isAdmin)
            {
                if (member != null)
                {
                    _dbContext.Entry(member).State = EntityState.Detached;
                    member.Role = ProjectRoles.Owner;
                    return member;
                }
                
                return new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = userId,
                    Role = ProjectRoles.Owner,
                    JoinedAt = DateTime.UtcNow
                };
            }

            return member;
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

        public async Task<List<ProjectResponse>> GetProjectDashboardsAsync(Guid userId)
        {
            var user = await _dbContext.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            bool isAdmin = user?.Role?.Name == "Admin";

            var query = _dbContext.ProjectMembers.AsNoTracking();

            if (!isAdmin)
            {
                query = query.Where(pm => pm.UserId == userId);
            }
            
            // To prevent duplicate projects for Admin when they are also a member, we group by Project
            // But Wait, if Admin, what UserRole should they see if they didn't join? Default to empty or Viewer.
            // Since ProjectResponse requires UserRole, if Admin is not a member, we can just say "Admin" or something.
            // A better way is:
            var projects = await _dbContext.Projects
                .AsNoTracking()
                .Include(p => p.Owner)
                .Include(p => p.ProjectMembers)
                .Include(p => p.Tasks)
                .Where(p => isAdmin || p.ProjectMembers.Any(pm => pm.UserId == userId))
                .Select(p => new ProjectResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    OwnerId = p.OwnerId,
                    OwnerEmail = p.Owner.Email,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    UserRole = p.ProjectMembers.FirstOrDefault(pm => pm.UserId == userId) != null 
                        ? p.ProjectMembers.FirstOrDefault(pm => pm.UserId == userId).Role 
                        : "Admin", // Fallback for Admin who isn't a member
                    MemberCount = p.ProjectMembers.Count(),
                    TotalTasks = p.Tasks.Count(),
                    CompletedTasks = p.Tasks.Count(t => t.Column != null && t.Column.IsCompletedStage)
                })
                .ToListAsync();

            return projects;
        }
    }
}
