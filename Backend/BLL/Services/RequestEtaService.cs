
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BLL.Services.ExternalServices;
using BLL.Services.Interfaces;
using DAL.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BLL.Services
{
    public class RequestEtaService : IRequestEtaService
    {
        private readonly IHelpRequestRepository _helpRequestRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly ITomTomRoutingService _tomTomRoutingService;
        private readonly IMemoryCache _cache;

        public RequestEtaService(
            IHelpRequestRepository helpRequestRepository,
            IVolunteerRepository volunteerRepository,
            ITomTomRoutingService tomTomRoutingService,
            IMemoryCache cache)
        {
            _helpRequestRepository = helpRequestRepository;
            _volunteerRepository = volunteerRepository;
            _tomTomRoutingService = tomTomRoutingService;
            _cache = cache;
        }

        public async Task<int?> GetEtaMinutesAsync(
            int requestId,
            CancellationToken cancellationToken = default)
        {
            var request = _helpRequestRepository.GetById(requestId);

            if (request == null ||
                !request.AssignedVolunteerId.HasValue ||
                (int)request.Status != 1 && (int)request.Status != 4)
            {
                return null;
            }

            var volunteer = _volunteerRepository.GetById(
                request.AssignedVolunteerId.Value);

            if (volunteer == null ||
                !volunteer.Latitude.HasValue ||
                !volunteer.Longitude.HasValue ||
                !volunteer.LastLocationUpdatedAt.HasValue)
            {
                return null;
            }

            string cacheKey =
                $"request-eta-{requestId}-{volunteer.LastLocationUpdatedAt.Value.Ticks}";

            if (_cache.TryGetValue(cacheKey, out int cachedMinutes))
            {
                return cachedMinutes;
            }

            var origin = new TomTomLocation(
                volunteer.Latitude.Value,
                volunteer.Longitude.Value);

            var destination = new TomTomLocation(
                request.Latitude,
                request.Longitude);

            var routes = await _tomTomRoutingService.CalculateRoutesAsync(
                new[] { origin },
                destination,
                cancellationToken);

            var route = routes.FirstOrDefault(r =>
                r.OriginIndex == 0 &&
                r.DestinationIndex == 0 &&
                r.TravelTimeInSeconds.HasValue &&
                string.IsNullOrEmpty(r.ErrorCode));

            if (route == null)
            {
                return null;
            }

            int etaMinutes = Math.Max(
                1,
                (int)Math.Ceiling(
                    route.TravelTimeInSeconds!.Value / 60.0));

            _cache.Set(
                cacheKey,
                etaMinutes,
                TimeSpan.FromSeconds(30));

            return etaMinutes;
        }
    }
}
