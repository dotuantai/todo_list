using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using API_v2.Repositorys.IRepositorys;

namespace API_v2.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly IProjectRepository _projectRepo;

        public NotificationHub(IProjectRepository projectRepo)
        {
            _projectRepo = projectRepo;
        }

        public async Task RegisterUser()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
        }

        public async Task JoinProject(string projectId)
        {
            if (!Guid.TryParse(projectId, out var projectGuid)) return;

            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId)) return;

            var member = await _projectRepo.GetMemberAsync(projectGuid, userId);
            if (member != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Project_{projectId}");
            }
        }

        public async Task LeaveProject(string projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        }
    }
}
