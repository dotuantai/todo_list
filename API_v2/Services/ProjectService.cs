using API_v2.Exceptions;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositorys.IRepositorys;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IUserRepository _userRepo;

        public ProjectService(IProjectRepository projectRepo, IUserRepository userRepo)
        {
            _projectRepo = projectRepo;
            _userRepo = userRepo;
        }

        public ProjectResponse CreateProject(CreateProjectRequest req, Guid currentUserId)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
            {
                throw ApiException.BadRequest("Project name cannot be empty.");
            }

            var user = _userRepo.GetById(currentUserId);
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
                Role = "Owner",
                JoinedAt = DateTime.UtcNow
            };
            _projectRepo.AddMember(member);
            _projectRepo.Save();

            var dbProject = _projectRepo.GetById(project.Id);
            return MapToProjectResponse(dbProject!, "Owner");
        }

        public List<ProjectResponse> GetProjectsForUser(Guid currentUserId)
        {
            var projects = _projectRepo.GetProjectsByUserId(currentUserId);
            var responses = new List<ProjectResponse>();

            foreach (var project in projects)
            {
                var member = _projectRepo.GetMember(project.Id, currentUserId);
                responses.Add(MapToProjectResponse(project, member?.Role));
            }

            return responses;
        }

        public ProjectResponse GetProjectDetail(Guid projectId, Guid currentUserId)
        {
            var project = _projectRepo.GetById(projectId);
            if (project is null)
            {
                throw ApiException.NotFound("Project does not exist.");
            }

            var member = _projectRepo.GetMember(projectId, currentUserId);
            if (member is null)
            {
                throw ApiException.Forbidden("You do not have access to this project.");
            }

            return MapToProjectResponse(project, member.Role);
        }

        public ProjectResponse UpdateProject(Guid projectId, UpdateProjectRequest req, Guid currentUserId)
        {
            var project = _projectRepo.GetById(projectId);
            if (project is null)
            {
                throw ApiException.NotFound("Project does not exist.");
            }

            var member = _projectRepo.GetMember(projectId, currentUserId);
            if (member is null || !member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase))
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

            _projectRepo.Save();
            return MapToProjectResponse(project, member.Role);
        }

        public void DeleteProject(Guid projectId, Guid currentUserId)
        {
            var project = _projectRepo.GetById(projectId);
            if (project is null)
            {
                throw ApiException.NotFound("Project does not exist.");
            }

            if (project.OwnerId != currentUserId)
            {
                throw ApiException.Forbidden("Only the project owner is allowed to delete the project.");
            }

            _projectRepo.Delete(project);
            _projectRepo.Save();
        }

        public List<MemberResponse> GetMembers(Guid projectId)
        {
            var members = _projectRepo.GetProjectMembers(projectId);
            return members.Select(m => new MemberResponse
            {
                UserId = m.UserId,
                Email = m.User?.Email,
                Role = m.Role,
                JoinedAt = m.JoinedAt
            }).ToList();
        }

        public MemberResponse AddMember(Guid projectId, AddMemberRequest req, Guid currentUserId)
        {
            if (string.IsNullOrWhiteSpace(req.Role) ||
                (!req.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) &&
                 !req.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase) &&
                 !req.Role.Equals("Viewer", StringComparison.OrdinalIgnoreCase)))
            {
                throw ApiException.BadRequest("Invalid role. Valid roles: Owner, Editor, Viewer.");
            }

            var targetUser = _userRepo.GetByEmail(req.Email?.Trim() ?? string.Empty);
            if (targetUser is null)
            {
                throw ApiException.NotFound($"No account found with email '{req.Email}'.");
            }

            var existingMember = _projectRepo.GetMember(projectId, targetUser.Id);
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

            _projectRepo.AddMember(member);
            _projectRepo.Save();

            return new MemberResponse
            {
                UserId = targetUser.Id,
                Email = targetUser.Email,
                Role = member.Role,
                JoinedAt = member.JoinedAt
            };
        }

        public MemberResponse UpdateMemberRole(Guid projectId, Guid userId, UpdateMemberRequest req, Guid currentUserId)
        {
            if (string.IsNullOrWhiteSpace(req.Role) ||
                (!req.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) &&
                 !req.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase) &&
                 !req.Role.Equals("Viewer", StringComparison.OrdinalIgnoreCase)))
            {
                throw ApiException.BadRequest("Invalid role.");
            }

            var project = _projectRepo.GetById(projectId);
            if (project is null)
            {
                throw ApiException.NotFound("Project does not exist.");
            }

            if (project.OwnerId == userId)
            {
                throw ApiException.BadRequest("Cannot change the role of the project owner.");
            }

            var targetMember = _projectRepo.GetMember(projectId, userId);
            if (targetMember is null)
            {
                throw ApiException.NotFound("Member does not belong to this project.");
            }

            targetMember.Role = req.Role;
            _projectRepo.Save();

            return new MemberResponse
            {
                UserId = targetMember.UserId,
                Email = targetMember.User?.Email,
                Role = targetMember.Role,
                JoinedAt = targetMember.JoinedAt
            };
        }

        public void RemoveMember(Guid projectId, Guid userId, Guid currentUserId)
        {
            var project = _projectRepo.GetById(projectId);
            if (project is null)
            {
                throw ApiException.NotFound("Project does not exist.");
            }

            if (project.OwnerId == userId)
            {
                throw ApiException.BadRequest("Cannot remove the project owner from the project.");
            }

            var targetMember = _projectRepo.GetMember(projectId, userId);
            if (targetMember is null)
            {
                throw ApiException.NotFound("Member does not belong to this project.");
            }

            _projectRepo.RemoveMember(targetMember);
            _projectRepo.Save();
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
