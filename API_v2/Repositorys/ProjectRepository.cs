using API_v2.Datas;
using API_v2.Models;
using API_v2.Repositorys.IRepositorys;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Repositorys
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _dbContext;

        public ProjectRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Project? GetById(Guid id)
        {
            return _dbContext.Projects
                .Include(p => p.Owner)
                .FirstOrDefault(p => p.Id == id);
        }

        public Project Add(Project project)
        {
            return _dbContext.Projects.Add(project).Entity;
        }

        public void Delete(Project project)
        {
            _dbContext.Projects.Remove(project);
        }

        public List<Project> GetProjectsByUserId(Guid userId)
        {
            return _dbContext.Projects
                .Include(p => p.Owner)
                .Where(p => p.ProjectMembers.Any(pm => pm.UserId == userId))
                .ToList();
        }

        public List<ProjectMember> GetProjectMembers(Guid projectId)
        {
            return _dbContext.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Include(pm => pm.User)
                .ToList();
        }

        public ProjectMember? GetMember(Guid projectId, Guid userId)
        {
            return _dbContext.ProjectMembers
                .FirstOrDefault(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public void AddMember(ProjectMember member)
        {
            _dbContext.ProjectMembers.Add(member);
        }

        public void RemoveMember(ProjectMember member)
        {
            _dbContext.ProjectMembers.Remove(member);
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}
