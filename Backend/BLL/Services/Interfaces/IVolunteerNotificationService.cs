using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IVolunteerNotificationService
    {
        Task NotifyNewOfferAsync(
            int volunteerId,
            int helpRequestId,
            CancellationToken cancellationToken = default);
    }
}