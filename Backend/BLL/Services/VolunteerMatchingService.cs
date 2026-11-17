
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
        private const int MaxActiveOffers = 5;
        private const double MaxAirDistanceKm = 15;

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

            var locationFreshnessCutoff =
                DateTime.UtcNow.AddMinutes(-5);

            var requiredEquipmentIds = helpRequest.RequiredEquipments
                .Select(e => e.EquipmentTypeId)
                .Distinct()
                .ToList();

            var nearbyVolunteers = new List<Volunteer>();
            var airDistances = new Dictionary<int, double>();

            _logger.LogInformation(
                "Starting volunteer matching for help request {HelpRequestId}. Volunteers found: {VolunteerCount}",
                helpRequestId,
                volunteers.Count);

            foreach (var volunteer in volunteers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string? rejectionReason = null;
                // לא לשלוח הצעה למתנדב שפתח בעצמו את בקשת העזרה
                if (volunteer.UserId == helpRequest.UserId)
                {
                    continue;
                }

                if (!volunteer.IsActive)
                {
                    rejectionReason = "Volunteer is inactive";
                }
                else if (!volunteer.Latitude.HasValue ||
                         !volunteer.Longitude.HasValue)
                {
                    rejectionReason = "Missing location coordinates";
                }
                else if (_helpRequestRepository.HasActiveAssignment(volunteer.Id))
                {
                    rejectionReason = "Has active assignment";
                }
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
                        volunteer.LastLocationUpdatedAt.Value >=
                            locationFreshnessCutoff;

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

                double? distanceKm =
                    CalculateDistanceKm(volunteer, helpRequest);

                if (!distanceKm.HasValue ||
                    distanceKm.Value > MaxAirDistanceKm)
                {
                    continue;
                }

                nearbyVolunteers.Add(volunteer);
                airDistances[volunteer.Id] = distanceKm.Value;

                _logger.LogInformation(
                    "Volunteer {VolunteerId} passed matching for help request {HelpRequestId}. Air distance: {DistanceKm:F2} km",
                    volunteer.Id,
                    helpRequestId,
                    distanceKm.Value);
            }

            _logger.LogInformation(
                "Air-distance matching completed for help request {HelpRequestId}. Eligible volunteers: {EligibleCount}",
                helpRequestId,
                nearbyVolunteers.Count);

            if (nearbyVolunteers.Count == 0)
                return new List<VolunteerMatchResult>();

            // Fewer than five eligible volunteers:
            // Send offers directly without calling TomTom.
            if (nearbyVolunteers.Count < MaxActiveOffers)
            {
                _logger.LogInformation(
                    "Only {EligibleCount} eligible volunteers found for help request {HelpRequestId}. Skipping TomTom and using air-distance ranking.",
                    nearbyVolunteers.Count,
                    helpRequestId);

                return BuildAirDistanceMatches(
                    nearbyVolunteers,
                    airDistances);
            }

            // Five or more eligible volunteers:
            // Use TomTom to rank them by driving distance.
            try
            {
                var origins = nearbyVolunteers
                    .Select(volunteer => new TomTomLocation(
                        volunteer.Latitude!.Value,
                        volunteer.Longitude!.Value))
                    .ToList();

                var destination = new TomTomLocation(
                    helpRequest.Latitude,
                    helpRequest.Longitude);

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
                    .ToList();

                // If even one volunteer has no valid route result,
                // use the air-distance fallback for everyone.
                if (validResults.Count != nearbyVolunteers.Count)
                {
                    _logger.LogWarning(
                        "TomTom returned incomplete or invalid route results for help request {HelpRequestId}. Expected: {ExpectedCount}, received: {ValidCount}. Falling back to air distance.",
                        helpRequestId,
                        nearbyVolunteers.Count,
                        validResults.Count);

                    return BuildAirDistanceMatches(
                        nearbyVolunteers,
                        airDistances);
                }

                return validResults
                    .Select(route =>
                    {
                        var volunteer =
                            nearbyVolunteers[route.OriginIndex];

                        double distanceScore = CalculateDistanceScore(
                            route.DistanceInMeters!.Value);

                        double reliabilityScore =
                            Math.Clamp(
                                volunteer.ReliabilityScore,
                                0,
                                100);

                        double matchScore =
                            distanceScore * 0.8 +
                            reliabilityScore * 0.2;

                        return new VolunteerMatchResult
                        {
                            Volunteer = volunteer,
                            DistanceInMeters =
                                route.DistanceInMeters.Value,
                            TravelTimeInSeconds =
                                route.TravelTimeInSeconds!.Value,
                            ReliabilityScore = reliabilityScore,
                            MatchScore = matchScore
                        };
                    })
                    .OrderByDescending(match => match.MatchScore)
                    .ToList();
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "TomTom routing failed for help request {HelpRequestId}. Falling back to air-distance ranking.",
                    helpRequestId);

                return BuildAirDistanceMatches(
                    nearbyVolunteers,
                    airDistances);
            }
        }

        public Task<List<HelpRequestMatchResult>>
            GetEligibleRequestsForVolunteerAsync(
                int volunteerId,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var volunteer =
                _volunteerRepository.GetByIdWithQualifications(volunteerId);

            if (volunteer == null || !volunteer.IsActive)
            {
                return Task.FromResult(
                    new List<HelpRequestMatchResult>());
            }

            var locationFreshnessCutoff =
                DateTime.UtcNow.AddMinutes(-5);

            if (!volunteer.LastLocationUpdatedAt.HasValue ||
                volunteer.LastLocationUpdatedAt.Value <
                    locationFreshnessCutoff)
            {
                return Task.FromResult(
                    new List<HelpRequestMatchResult>());
            }

            if (!volunteer.Latitude.HasValue ||
                !volunteer.Longitude.HasValue)
            {
                return Task.FromResult(
                    new List<HelpRequestMatchResult>());
            }

            if (_helpRequestRepository.HasActiveAssignment(volunteerId))
            {
                return Task.FromResult(
                    new List<HelpRequestMatchResult>());
            }

            var openRequests = _helpRequestRepository.GetOpenRequests();

            var matches = new List<HelpRequestMatchResult>();

            foreach (var request in openRequests)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (request.Status != RequestStatus.Open ||
                    request.AssignedVolunteerId != null ||
                    request.HelpTypeId == null)
                {
                    continue;
                }

                bool hasRequiredHelpType =
                    volunteer.VolunteerHelpTypes.Any(
                        ht => ht.HelpTypeId == request.HelpTypeId);

                if (!hasRequiredHelpType)
                    continue;

                var requiredEquipmentIds = request.RequiredEquipments
                    .Select(e => e.EquipmentTypeId)
                    .Distinct()
                    .ToList();

                bool hasAllRequiredEquipment =
                    requiredEquipmentIds.All(requiredId =>
                        volunteer.VolunteerEquipments.Any(
                            equipment =>
                                equipment.EquipmentTypeId == requiredId));

                if (!hasAllRequiredEquipment)
                    continue;

                double? distanceKm =
                    CalculateDistanceKm(volunteer, request);

                if (!distanceKm.HasValue ||
                    distanceKm.Value > MaxAirDistanceKm)
                {
                    continue;
                }

                double distanceInMeters =
                    distanceKm.Value * 1000;

                double distanceScore =
                    CalculateDistanceScore(distanceInMeters);

                double reliabilityScore =
                    Math.Clamp(volunteer.ReliabilityScore, 0, 100);

                double matchScore =
                    distanceScore * 0.8 +
                    reliabilityScore * 0.2;

                matches.Add(new HelpRequestMatchResult
                {
                    HelpRequest = request,
                    DistanceInMeters =
                        (int)Math.Round(distanceInMeters),
                    TravelTimeInSeconds = 0,
                    ReliabilityScore = reliabilityScore,
                    MatchScore = matchScore
                });
            }

            var orderedMatches = matches
                .OrderByDescending(match => match.MatchScore)
                .ToList();

            return Task.FromResult(orderedMatches);
        }

        private static List<VolunteerMatchResult>
            BuildAirDistanceMatches(
                List<Volunteer> volunteers,
                Dictionary<int, double> airDistances)
        {
            return volunteers
                .Select(volunteer =>
                {
                    double distanceInMeters =
                        airDistances[volunteer.Id] * 1000;

                    double distanceScore =
                        CalculateDistanceScore(distanceInMeters);

                    double reliabilityScore =
                        Math.Clamp(volunteer.ReliabilityScore, 0, 100);

                    double matchScore =
                        distanceScore * 0.8 +
                        reliabilityScore * 0.2;

                    return new VolunteerMatchResult
                    {
                        Volunteer = volunteer,
                        DistanceInMeters =
                            (int)Math.Round(distanceInMeters),
                        TravelTimeInSeconds = 0,
                        ReliabilityScore = reliabilityScore,
                        MatchScore = matchScore
                    };
                })
                .OrderByDescending(match => match.MatchScore)
                .ToList();
        }

        private static double CalculateDistanceScore(
            double distanceInMeters)
        {
            return Math.Clamp(
                100.0 * (15000 - distanceInMeters) / 15000,
                0,
                100);
        }

        private static double? CalculateDistanceKm(
            Volunteer volunteer,
            HelpRequest request)
        {
            if (!volunteer.Latitude.HasValue ||
                !volunteer.Longitude.HasValue)
            {
                return null;
            }

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
