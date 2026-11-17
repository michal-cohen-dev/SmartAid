
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DAL.Entities;
using DAL.Repositories.Interfaces;

using BLL.Services.Interfaces;
using BLL.Services.ExternalServices;
using BLL.Services.Models;
using BLL.Exceptions;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class VolunteerMatchingService : IVolunteerMatchingService
    {
        private const int MaxRoutesPerBatch = 100;

        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IHelpRequestRepository _helpRequestRepository;
        private readonly ITomTomRoutingService _tomTomRoutingService;
        private readonly ILogger<VolunteerMatchingService> _logger;

        public VolunteerMatchingService(
    IVolunteerRepository volunteerRepository,
    IHelpRequestRepository helpRequestRepository,
    ITomTomRoutingService tomTomRoutingService,
    ILogger<VolunteerMatchingService> logger)
        {
            _volunteerRepository = volunteerRepository;
            _helpRequestRepository = helpRequestRepository;
            _tomTomRoutingService = tomTomRoutingService;
            _logger = logger;
        }

        public async Task<List<VolunteerMatchResult>> GetEligibleVolunteersAsync(
            int helpRequestId,
            CancellationToken cancellationToken = default)
        {
            var helpRequest = _helpRequestRepository.GetById(helpRequestId);

            if (helpRequest == null)
                throw new NotFoundException("Help request not found.");

            if (helpRequest.Status != RequestStatus.Open ||
                helpRequest.AssignedVolunteerId != null)
            {
                return new List<VolunteerMatchResult>();
            }

            if (helpRequest.HelpTypeId == null)
                throw new BusinessRuleException(
                    "The help request must have a help type.");

            var volunteers = _volunteerRepository.GetAllWithQualifications();

            var locationFreshnessCutoff = DateTime.UtcNow.AddMinutes(-5);

            var requiredEquipmentIds = helpRequest.RequiredEquipments
                .Select(e => e.EquipmentTypeId)
                .Distinct()
                .ToList();

            var eligibleVolunteers = new List<Volunteer>();

            _logger.LogInformation(
                "Starting volunteer matching for help request {HelpRequestId}. Volunteers found: {VolunteerCount}",
                helpRequestId,
                volunteers.Count);

            foreach (var volunteer in volunteers)
            {
                string rejectionReason = null;

                if (!volunteer.IsActive)
                    rejectionReason = "Volunteer is inactive";
                else if (!volunteer.Latitude.HasValue ||
                         !volunteer.Longitude.HasValue)
                    rejectionReason = "Missing location coordinates";
                else if (_helpRequestRepository.HasActiveAssignment(volunteer.Id))
                    rejectionReason = "Has active assignment";
                else
                {
                    bool hasRequiredHelpType =
                        volunteer.VolunteerHelpTypes.Any(
                            ht => ht.HelpTypeId == helpRequest.HelpTypeId);

                    bool hasAllRequiredEquipment =
                        requiredEquipmentIds.All(requiredId =>
                            volunteer.VolunteerEquipments.Any(
                                equipment =>
                                    equipment.EquipmentTypeId == requiredId));

                    bool hasFreshLocation =
                        volunteer.LastLocationUpdatedAt.HasValue &&
                        volunteer.LastLocationUpdatedAt.Value >= locationFreshnessCutoff;

                    if (!hasRequiredHelpType)
                        rejectionReason = "Missing required help type";
                    else if (!hasAllRequiredEquipment)
                        rejectionReason = "Missing required equipment";
                    else if (!hasFreshLocation)
                        rejectionReason = "Location is not fresh";
                }

                if (rejectionReason != null)
                {
                    _logger.LogInformation(
                        "Volunteer {VolunteerId} rejected for help request {HelpRequestId}: {Reason}",
                        volunteer.Id,
                        helpRequestId,
                        rejectionReason);

                    continue;
                }

                eligibleVolunteers.Add(volunteer);

                _logger.LogInformation(
                    "Volunteer {VolunteerId} passed initial matching for help request {HelpRequestId}",
                    volunteer.Id,
                    helpRequestId);
            }

            _logger.LogInformation(
                "Initial matching completed for help request {HelpRequestId}. Eligible volunteers: {EligibleCount}",
                helpRequestId,
                eligibleVolunteers.Count);

            const double maxAirDistanceKm = 15;

            var nearbyVolunteers = eligibleVolunteers
                .Where(volunteer =>
                {
                    double? distance = CalculateDistanceKm(volunteer, helpRequest);

                    return distance.HasValue &&
                           distance.Value <= maxAirDistanceKm;
                })
                .ToList();

            if (!nearbyVolunteers.Any())
                return new List<VolunteerMatchResult>();

            var origins = nearbyVolunteers
                .Select(volunteer => new TomTomLocation(
                    volunteer.Latitude!.Value,
                    volunteer.Longitude!.Value))
                .ToList();

            var destination = new TomTomLocation(
                helpRequest.Latitude,
                helpRequest.Longitude);

            // TomTom allows up to 100 routes per request.
            // Split volunteers into batches and restore their original indexes.
            var routes = new List<TomTomRouteResult>();

            for (int startIndex = 0;
                 startIndex < origins.Count;
                 startIndex += MaxRoutesPerBatch)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batchOrigins = origins
                    .Skip(startIndex)
                    .Take(MaxRoutesPerBatch)
                    .ToList();

                var batchRoutes =
                    await _tomTomRoutingService.CalculateRoutesAsync(
                        batchOrigins,
                        destination,
                        cancellationToken);

                routes.AddRange(
                    batchRoutes.Select(route => new TomTomRouteResult
                    {
                        OriginIndex = route.OriginIndex + startIndex,
                        DestinationIndex = route.DestinationIndex,
                        DistanceInMeters = route.DistanceInMeters,
                        TravelTimeInSeconds = route.TravelTimeInSeconds,
                        TrafficDelayInSeconds = route.TrafficDelayInSeconds,
                        ErrorCode = route.ErrorCode
                    }));
            }

            var validResults = routes
                .Where(route =>
                    route.DestinationIndex == 0 &&
                    route.OriginIndex >= 0 &&
                    route.OriginIndex < nearbyVolunteers.Count &&
                    route.TravelTimeInSeconds.HasValue &&
                    route.DistanceInMeters.HasValue &&
                    string.IsNullOrEmpty(route.ErrorCode))
                .Select(route => new
                {
                    Volunteer = nearbyVolunteers[route.OriginIndex],
                    Distance = route.DistanceInMeters!.Value,
                    TravelTime = route.TravelTimeInSeconds!.Value
                })
                .ToList();

            if (!validResults.Any())
                return new List<VolunteerMatchResult>();

            return validResults
                .Select(x =>
                {
                    double distanceScore = Math.Clamp(
                        100.0 * (15000 - x.Distance) / 15000,
                        0,
                        100);

                    double reliabilityScore =
                        Math.Clamp(x.Volunteer.ReliabilityScore, 0, 100);

                    double matchScore =
                        distanceScore * 0.8 + reliabilityScore * 0.2;

                    return new VolunteerMatchResult
                    {
                        Volunteer = x.Volunteer,
                        DistanceInMeters = x.Distance,
                        TravelTimeInSeconds = x.TravelTime,
                        ReliabilityScore = reliabilityScore,
                        MatchScore = matchScore
                    };
                })
                .OrderByDescending(x => x.MatchScore)
                .ToList();
        }

        public async Task<List<HelpRequestMatchResult>>
            GetEligibleRequestsForVolunteerAsync(
                int volunteerId,
                CancellationToken cancellationToken = default)
        {
            var volunteer =
                _volunteerRepository.GetByIdWithQualifications(volunteerId);

            if (volunteer == null || !volunteer.IsActive)
                return new List<HelpRequestMatchResult>();

            var locationFreshnessCutoff = DateTime.UtcNow.AddMinutes(-5);

            if (!volunteer.LastLocationUpdatedAt.HasValue ||
                volunteer.LastLocationUpdatedAt.Value < locationFreshnessCutoff)
            {
                return new List<HelpRequestMatchResult>();
            }

            if (volunteer.Latitude == null || volunteer.Longitude == null)
                return new List<HelpRequestMatchResult>();

            if (_helpRequestRepository.HasActiveAssignment(volunteerId))
                return new List<HelpRequestMatchResult>();

            var openRequests = _helpRequestRepository.GetOpenRequests();

            var eligibleRequests = openRequests
                .Where(request =>
                {
                    if (request.HelpTypeId == null ||
                        request.AssignedVolunteerId != null)
                        return false;

                    bool hasRequiredHelpType =
                        volunteer.VolunteerHelpTypes.Any(
                            ht => ht.HelpTypeId == request.HelpTypeId);

                    var requiredEquipmentIds = request.RequiredEquipments
                        .Select(e => e.EquipmentTypeId)
                        .Distinct()
                        .ToList();

                    bool hasAllRequiredEquipment =
                        requiredEquipmentIds.All(requiredId =>
                            volunteer.VolunteerEquipments.Any(
                                equipment =>
                                    equipment.EquipmentTypeId == requiredId));

                    return hasRequiredHelpType && hasAllRequiredEquipment;
                })
                .Where(request =>
                {
                    double? distance = CalculateDistanceKm(volunteer, request);

                    return distance.HasValue && distance.Value <= 15;
                })
                .ToList();

            if (!eligibleRequests.Any())
                return new List<HelpRequestMatchResult>();

            var origin = new TomTomLocation(
                volunteer.Latitude.Value,
                volunteer.Longitude.Value);

            var destinations = eligibleRequests
                .Select(request => new TomTomLocation(
                    request.Latitude,
                    request.Longitude))
                .ToList();

            // TomTom allows up to 100 routes per request.
            // Split requests into batches and restore their original indexes.
            var routes = new List<TomTomRouteResult>();

            for (int startIndex = 0;
                 startIndex < destinations.Count;
                 startIndex += MaxRoutesPerBatch)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batchDestinations = destinations
                    .Skip(startIndex)
                    .Take(MaxRoutesPerBatch)
                    .ToList();

                var batchRoutes =
                    await _tomTomRoutingService
                        .CalculateRoutesToMultipleDestinationsAsync(
                            origin,
                            batchDestinations,
                            cancellationToken);

                routes.AddRange(
                    batchRoutes.Select(route => new TomTomRouteResult
                    {
                        OriginIndex = route.OriginIndex,
                        DestinationIndex = route.DestinationIndex + startIndex,
                        DistanceInMeters = route.DistanceInMeters,
                        TravelTimeInSeconds = route.TravelTimeInSeconds,
                        TrafficDelayInSeconds = route.TrafficDelayInSeconds,
                        ErrorCode = route.ErrorCode
                    }));
            }

            var validResults = routes
                .Where(route =>
                    route.OriginIndex == 0 &&
                    route.DestinationIndex >= 0 &&
                    route.DestinationIndex < eligibleRequests.Count &&
                    route.TravelTimeInSeconds.HasValue &&
                    route.DistanceInMeters.HasValue &&
                    string.IsNullOrEmpty(route.ErrorCode))
                .Select(route => new
                {
                    Request = eligibleRequests[route.DestinationIndex],
                    Distance = route.DistanceInMeters!.Value,
                    TravelTime = route.TravelTimeInSeconds!.Value
                })
                .ToList();

            if (!validResults.Any())
                return new List<HelpRequestMatchResult>();

            double reliabilityScore =
                Math.Clamp(volunteer.ReliabilityScore, 0, 100);

            return validResults
                .Select(x =>
                {
                    double distanceScore = Math.Clamp(
                        100.0 * (15000 - x.Distance) / 15000,
                        0,
                        100);

                    double matchScore =
                        distanceScore * 0.8 + reliabilityScore * 0.2;

                    return new HelpRequestMatchResult
                    {
                        HelpRequest = x.Request,
                        DistanceInMeters = x.Distance,
                        TravelTimeInSeconds = x.TravelTime,
                        ReliabilityScore = reliabilityScore,
                        MatchScore = matchScore
                    };
                })
                .OrderByDescending(x => x.MatchScore)
                .ToList();
        }

        private static double? CalculateDistanceKm(
            Volunteer volunteer,
            HelpRequest request)
        {
            if (volunteer.Latitude == null ||
                volunteer.Longitude == null)
                return null;

            const double earthRadiusKm = 6371;

            double volunteerLat =
                volunteer.Latitude.Value * Math.PI / 180;

            double volunteerLon =
                volunteer.Longitude.Value * Math.PI / 180;

            double requestLat =
                request.Latitude * Math.PI / 180;

            double requestLon =
                request.Longitude * Math.PI / 180;

            double deltaLat = requestLat - volunteerLat;
            double deltaLon = requestLon - volunteerLon;

            double a =
                Math.Pow(Math.Sin(deltaLat / 2), 2) +
                Math.Cos(volunteerLat) *
                Math.Cos(requestLat) *
                Math.Pow(Math.Sin(deltaLon / 2), 2);

            a = Math.Clamp(a, 0, 1);

            double c = 2 * Math.Asin(Math.Sqrt(a));

            return earthRadiusKm * c;
        }
    }
}
