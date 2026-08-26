using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Exceptions;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Models.Constants;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IUserRepository _userRepo;
        private readonly INotificationService _notificationService;
        private readonly IProjectColumnRepository _columnRepo;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(
            IProjectRepository projectRepo, 
            IUserRepository userRepo,
            INotificationService notificationService,
            IProjectColumnRepository columnRepo,
            ILogger<ProjectService> logger)
        {
            _projectRepo = projectRepo;
            _userRepo = userRepo;
            _notificationService = notificationService;
            _columnRepo = columnRepo;
            _logger = logger;
        }

        public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest req, Guid currentUserId)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
            {
                throw ApiException.BadRequest("Project name cannot be empty.");
            }

            var user = await _userRepo.GetByIdAsync(currentUserId);
            if (user is null)
            {
                throw ApiException.Unauthorized("Account information not found.");
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = req.Name.Trim(),
                Description = req.Description?.Trim(),
                OwnerId = currentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _projectRepo.Add(project);

            var member = new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                UserId = currentUserId,
                Role = ProjectRoles.Owner,
                JoinedAt = DateTime.UtcNow
            };
            _projectRepo.AddMember(member);

            // Seed default columns
            _columnRepo.Add(new ProjectColumn { ProjectId = project.Id, Name = "To Do", Order = 0, IsCompletedStage = false, CreatedAt = DateTime.UtcNow });
            _columnRepo.Add(new ProjectColumn { ProjectId = project.Id, Name = "In Progress", Order = 1, IsCompletedStage = false, CreatedAt = DateTime.UtcNow });
            _columnRepo.Add(new ProjectColumn { ProjectId = project.Id, Name = "Done", Order = 2, IsCompletedStage = true, CreatedAt = DateTime.UtcNow });
            _columnRepo.Add(new ProjectColumn { ProjectId = project.Id, Name = "Closed", Order = 3, IsCompletedStage = true, CreatedAt = DateTime.UtcNow });

            await _projectRepo.SaveAsync();

            var dbProject = await _projectRepo.GetByIdAsync(project.Id);
            return MapToProjectResponse(dbProject!, ProjectRoles.Owner);
        }

        public async Task<PagedResponse<ProjectResponse>> GetProjectsForUserAsync(Guid currentUserId, int page, int pageSize)
        {
            return await _projectRepo.GetProjectDashboardsAsync(currentUserId, page, pageSize);
        }

        public async Task<ProjectResponse> GetProjectDetailAsync(Guid projectId, Guid currentUserId)
        {
            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project is null)
            {
                throw ApiException.NotFound("Project does not exist.");
            }

            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member is null)
            {
                throw ApiException.Forbidden("You do not have access to this project.");
            }

            return MapToProjectResponse(project, member.Role);
        }

        public async Task<ProjectResponse> UpdateProjectAsync(Guid projectId, UpdateProjectRequest req, Guid currentUserId)
        {
            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project is null)
            {
                throw ApiException.NotFound("Project does not exist.");
            }

            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member is null || !member.Role.Equals(ProjectRoles.Owner, StringComparison.OrdinalIgnoreCase))
            {
                throw ApiException.Forbidden("Only the Owner is allowed to edit project information.");
            }

            if (string.IsNullOrWhiteSpace(req.Name))
            {
                throw ApiException.BadRequest("Project name cannot be empty.");
            }

            project.Name = req.Name.Trim();
            project.Description = req.Description?.Trim();
            project.UpdatedAt = DateTime.UtcNow;

            await _projectRepo.SaveAsync();
            return MapToProjectResponse(project, member.Role);
        }

        public async Task DeleteProjectAsync(Guid projectId, Guid currentUserId)
        {
            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project is null)
            {
                throw ApiException.NotFound("Project does not exist.");
            }

            if (project.OwnerId != currentUserId)
            {
                throw ApiException.Forbidden("Only the project owner is allowed to delete the project.");
            }

            _projectRepo.Delete(project);
            await _projectRepo.SaveAsync();
        }

        public async Task<List<MemberResponse>> GetMembersAsync(Guid projectId, Guid currentUserId)
        {
            var member = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (member is null)
            {
                throw ApiException.Forbidden("You do not have access to this project.");
            }

            var members = await _projectRepo.GetProjectMembersAsync(projectId);
            return members.Select(m => new MemberResponse
            {
                UserId = m.UserId,
                Email = m.User?.Email,
                Role = m.Role,
                JoinedAt = m.JoinedAt
            }).ToList();
        }

        public async Task<MemberResponse> AddMemberAsync(Guid projectId, AddMemberRequest req, Guid currentUserId)
        {
            var currentMember = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (currentMember is null || !currentMember.Role.Equals(ProjectRoles.Owner, StringComparison.OrdinalIgnoreCase))
            {
                throw ApiException.Forbidden("Only the project owner is allowed to manage members.");
            }

            if (!ProjectRoles.IsValid(req.Role))
            {
                throw ApiException.BadRequest("Invalid role. Valid roles: Owner, Manager, Member.");
            }

            var targetUser = await _userRepo.GetByEmailAsync(req.Email?.Trim() ?? string.Empty);
            if (targetUser is null)
            {
                throw ApiException.NotFound($"No account found with email '{req.Email}'.");
            }

            var existingMember = await _projectRepo.GetMemberAsync(projectId, targetUser.Id);
            if (existingMember is not null)
            {
                throw ApiException.Conflict("This user is already a member of the project.");
            }

            var member = new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = targetUser.Id,
                Role = req.Role,
                JoinedAt = DateTime.UtcNow
            };

            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project is null)
            {
                throw ApiException.NotFound("Project not found.");
            }

            _projectRepo.AddMember(member);
            await _projectRepo.SaveAsync();

            try
            {
                await _notificationService.CreateAndSendNotificationAsync(
                    targetUser.Id,
                    "New Project Invitation",
                    $"You have been added to the project '{project.Name}' as a {req.Role}.",
                    "ProjectInvited",
                    projectId.ToString()
                );
            }
            catch (Exception ex)
            {
                // Soft fail if SignalR hub or database notification logic encounters issues
                // so that the member addition itself is not rolled back.
                _logger.LogWarning(ex, "Failed to send new project invitation notification.");
            }

            return new MemberResponse
            {
                UserId = targetUser.Id,
                Email = targetUser.Email,
                Role = member.Role,
                JoinedAt = member.JoinedAt
            };
        }

        public async Task<MemberResponse> UpdateMemberRoleAsync(Guid projectId, Guid userId, UpdateMemberRequest req, Guid currentUserId)
        {
            var currentMember = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (currentMember is null || !currentMember.Role.Equals(ProjectRoles.Owner, StringComparison.OrdinalIgnoreCase))
            {
                throw ApiException.Forbidden("Only the project owner is allowed to manage members.");
            }

            if (!ProjectRoles.IsValid(req.Role))
            {
                throw ApiException.BadRequest("Invalid role. Valid roles: Owner, Manager, Member.");
            }

            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project is null)
            {
                throw ApiException.NotFound("Project does not exist.");
            }

            if (project.OwnerId == userId)
            {
                throw ApiException.BadRequest("Cannot change the role of the project owner.");
            }

            var targetMember = await _projectRepo.GetMemberAsync(projectId, userId);
            if (targetMember is null)
            {
                throw ApiException.NotFound("Member does not belong to this project.");
            }

            targetMember.Role = req.Role;
            await _projectRepo.SaveAsync();

            return new MemberResponse
            {
                UserId = targetMember.UserId,
                Email = targetMember.User?.Email,
                Role = targetMember.Role,
                JoinedAt = targetMember.JoinedAt
            };
        }

        public async Task RemoveMemberAsync(Guid projectId, Guid userId, Guid currentUserId)
        {
            var currentMember = await _projectRepo.GetMemberAsync(projectId, currentUserId);
            if (currentMember is null || !currentMember.Role.Equals(ProjectRoles.Owner, StringComparison.OrdinalIgnoreCase))
            {
                throw ApiException.Forbidden("Only the project owner is allowed to manage members.");
            }

            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project is null)
            {
                throw ApiException.NotFound("Project does not exist.");
            }

            if (project.OwnerId == userId)
            {
                throw ApiException.BadRequest("Cannot remove the project owner from the project.");
            }

            var targetMember = await _projectRepo.GetMemberAsync(projectId, userId);
            if (targetMember is null)
            {
                throw ApiException.NotFound("Member does not belong to this project.");
            }

            _projectRepo.RemoveMember(targetMember);
            await _projectRepo.SaveAsync();
        }

        private ProjectResponse MapToProjectResponse(Project project, string? userRole)
        {
            return new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                OwnerId = project.OwnerId,
                OwnerEmail = project.Owner?.Email,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                UserRole = userRole
            };
        }
    }
}
