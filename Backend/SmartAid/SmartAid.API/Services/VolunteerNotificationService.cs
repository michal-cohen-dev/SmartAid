
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using SmartAid.API.Hubs;

namespace SmartAid.API.Services
{
    public class VolunteerNotificationService : IVolunteerNotificationService
    {
        private readonly IHubContext<NotificationsHub> _hubContext;

        public VolunteerNotificationService(
            IHubContext<NotificationsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyNewOfferAsync(
            int volunteerId,
            int helpRequestId,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients
                .Group($"volunteer:{volunteerId}")
                .SendAsync(
                    "NewOffer",
                    new
                    {
                        HelpRequestId = helpRequestId
                    },
                    cancellationToken);
        }
    }
}
