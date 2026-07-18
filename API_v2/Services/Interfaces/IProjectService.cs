using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface IProjectService
    {
        ProjectResponse CreateProject(CreateProjectRequest req, Guid currentUserId);
        List<ProjectResponse> GetProjectsForUser(Guid currentUserId);
        ProjectResponse GetProjectDetail(Guid projectId, Guid currentUserId);
        ProjectResponse UpdateProject(Guid projectId, UpdateProjectRequest req, Guid currentUserId);
        void DeleteProject(Guid projectId, Guid currentUserId);
        List<MemberResponse> GetMembers(Guid projectId);
        MemberResponse AddMember(Guid projectId, AddMemberRequest req, Guid currentUserId);
        MemberResponse UpdateMemberRole(Guid projectId, Guid userId, UpdateMemberRequest req, Guid currentUserId);
        void RemoveMember(Guid projectId, Guid userId, Guid currentUserId);
    }
}
