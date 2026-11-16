
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SmartAid.API.Hubs
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // זיהוי המשתמש המחובר
            var userId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // צירוף המשתמש לקבוצה האישית שלו
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"user:{userId}");
            }

            // שמירת התמיכה הקיימת בקבוצות מתנדבים
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
