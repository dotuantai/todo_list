using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace API_v2.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task RegisterUser(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}
