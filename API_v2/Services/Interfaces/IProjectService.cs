using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest req, Guid currentUserId);
        Task<PagedResponse<ProjectResponse>> GetProjectsForUserAsync(Guid currentUserId, int page, int pageSize);
        Task<ProjectResponse> GetProjectDetailAsync(Guid projectId, Guid currentUserId);
        Task<ProjectResponse> UpdateProjectAsync(Guid projectId, UpdateProjectRequest req, Guid currentUserId);
        Task DeleteProjectAsync(Guid projectId, Guid currentUserId);
        Task<List<MemberResponse>> GetMembersAsync(Guid projectId, Guid currentUserId);
        Task<MemberResponse> AddMemberAsync(Guid projectId, AddMemberRequest req, Guid currentUserId);
        Task<MemberResponse> UpdateMemberRoleAsync(Guid projectId, Guid userId, UpdateMemberRequest req, Guid currentUserId);
        Task RemoveMemberAsync(Guid projectId, Guid userId, Guid currentUserId);
    }
}
