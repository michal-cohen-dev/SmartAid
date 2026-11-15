
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using BLL.Services.ExternalServices;

namespace BLL.Services.Interfaces
{
    public interface ITomTomRoutingService
    {
        Task<IReadOnlyList<TomTomRouteResult>> CalculateRoutesAsync(
            IReadOnlyCollection<TomTomLocation> origins,
            TomTomLocation destination,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TomTomRouteResult>> CalculateRoutesToMultipleDestinationsAsync(
            TomTomLocation origin,
            IReadOnlyCollection<TomTomLocation> destinations,
            CancellationToken cancellationToken = default);
    }
}
