
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartAid.API.Hubs
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var volunteerId = Context.User?
                .FindFirst("volunteerId")?.Value;

            if (!string.IsNullOrWhiteSpace(volunteerId))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"volunteer:{volunteerId}");
            }

            await base.OnConnectedAsync();
        }
    }
}
