
using System;
using System.Threading;
using System.Threading.Tasks;
using BLL.Services.ExternalServices;
using BLL.Services.Interfaces;
using DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace SmartAid.API.Services
{
    public class EstimatedArrivalService
    {
        private static readonly TimeSpan CacheDuration =
            TimeSpan.FromSeconds(30);

        private static readonly TimeSpan LocationMaxAge =
            TimeSpan.FromMinutes(5);

        private readonly ITomTomRoutingService _tomTomRoutingService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EstimatedArrivalService> _logger;

        public EstimatedArrivalService(
            ITomTomRoutingService tomTomRoutingService,
            IMemoryCache cache,
            ILogger<EstimatedArrivalService> logger)
        {
            _tomTomRoutingService = tomTomRoutingService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<int?> GetEstimatedArrivalSecondsAsync(
            HelpRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                return null;

            // ETA is relevant only while the request is assigned
            // or the volunteer is treating it.
            if (request.Status != 1 && request.Status != 4)
                return null;

            if (!request.AssignedVolunteerLatitude.HasValue ||
                !request.AssignedVolunteerLongitude.HasValue ||
                !request.AssignedVolunteerLastLocationUpdatedAt.HasValue)
            {
                return null;
            }

            double volunteerLatitude =
                request.AssignedVolunteerLatitude.Value;

            double volunteerLongitude =
                request.AssignedVolunteerLongitude.Value;

            if (volunteerLatitude < -90 || volunteerLatitude > 90 ||
                volunteerLongitude < -180 || volunteerLongitude > 180 ||
                request.Latitude < -90 || request.Latitude > 90 ||
                request.Longitude < -180 || request.Longitude > 180)
            {
                return null;
            }

            DateTime locationUpdatedAt =
                request.AssignedVolunteerLastLocationUpdatedAt.Value;

            if (locationUpdatedAt < DateTime.UtcNow - LocationMaxAge)
                return null;

            string cacheKey = $"request-eta-{request.Id}";

            if (_cache.TryGetValue(
                cacheKey,
                out CachedEta? cachedEta) &&
                cachedEta != null &&
                cachedEta.VolunteerLatitude == volunteerLatitude &&
                cachedEta.VolunteerLongitude == volunteerLongitude &&
                cachedEta.LocationUpdatedAt == locationUpdatedAt)
            {
                return cachedEta.Seconds;
            }

            try
            {
                var origins = new[]
                {
                    new TomTomLocation(
                        volunteerLatitude,
                        volunteerLongitude)
                };

                var destination = new TomTomLocation(
                    request.Latitude,
                    request.Longitude);

                var routes = await _tomTomRoutingService.CalculateRoutesAsync(
                    origins,
                    destination,
                    cancellationToken);

                var route = routes.Count > 0 ? routes[0] : null;

                if (route == null || !route.TravelTimeInSeconds.HasValue)
                    return null;

                int estimatedSeconds = route.TravelTimeInSeconds.Value;

                var newCachedEta = new CachedEta
                {
                    VolunteerLatitude = volunteerLatitude,
                    VolunteerLongitude = volunteerLongitude,
                    LocationUpdatedAt = locationUpdatedAt,
                    Seconds = estimatedSeconds
                };

                _cache.Set(
                    cacheKey,
                    newCachedEta,
                    CacheDuration);

                return estimatedSeconds;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not calculate ETA for help request {RequestId}.",
                    request.Id);

                return null;
            }
        }

        private sealed class CachedEta
        {
            public double VolunteerLatitude { get; set; }
            public double VolunteerLongitude { get; set; }
            public DateTime LocationUpdatedAt { get; set; }
            public int Seconds { get; set; }
        }
    }
}
