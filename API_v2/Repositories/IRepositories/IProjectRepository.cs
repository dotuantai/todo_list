using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models;

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
        Task<bool> IsSystemAdminAsync(Guid userId);
        void AddMember(ProjectMember member);
        void RemoveMember(ProjectMember member);
        Task SaveAsync();
        Task<List<ProjectMember>> GetProjectMembersWithProjectsByUserIdAsync(Guid userId);
        Task<(List<ProjectDashboardRecord> Items, int TotalCount)> GetProjectDashboardsAsync(Guid userId, int page, int pageSize);
    }

    public sealed record ProjectDashboardRecord(
        Guid Id, string Name, string? Description, Guid OwnerId, string OwnerEmail,
        DateTime CreatedAt, DateTime UpdatedAt, string UserRole,
        int MemberCount, int CompletedTasks, int TotalTasks);
}
