using API_v2.Models;

namespace API_v2.Repositorys.IRepositorys
{
    public interface IProjectRepository
    {
        Project? GetById(Guid id);
        Project Add(Project project);
        void Delete(Project project);
        List<Project> GetProjectsByUserId(Guid userId);
        List<ProjectMember> GetProjectMembers(Guid projectId);
        ProjectMember? GetMember(Guid projectId, Guid userId);
        void AddMember(ProjectMember member);
        void RemoveMember(ProjectMember member);
        void Save();
    }
}
