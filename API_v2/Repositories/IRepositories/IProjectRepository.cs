using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models;
using API_v2.Models.DTOs;

namespace API_v2.Repositories.IRepositories
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(Guid id);
        Project Add(Project project);
        void Delete(Project project);
        Task<List<Project>> GetProjectsByUserIdAsync(Guid userId);
        Task<List<ProjectMember>> GetProjectMembersAsync(Guid projectId);
        Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId);
        void AddMember(ProjectMember member);
        void RemoveMember(ProjectMember member);
        Task SaveAsync();
        Task<List<ProjectMember>> GetProjectMembersWithProjectsByUserIdAsync(Guid userId);
        Task<List<ProjectResponse>> GetProjectDashboardsAsync(Guid userId);
    }
}
