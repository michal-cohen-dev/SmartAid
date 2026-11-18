using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IEmailNotificationService
    {
        Task SendNewOfferAsync(
            string recipientEmail,
            CancellationToken cancellationToken = default);
    }
}
