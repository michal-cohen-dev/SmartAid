
using BLL.Services.ExternalServices;

namespace BLL.Services.Interfaces
{
    public interface ITomTomRoutingService
    {
        Task<IReadOnlyList<TomTomRouteResult>> CalculateRoutesAsync(
            IReadOnlyCollection<TomTomLocation> origins,
            TomTomLocation destination,
            CancellationToken cancellationToken = default);
    }
}
