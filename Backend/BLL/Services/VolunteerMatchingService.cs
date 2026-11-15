
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

namespace BLL.Services
{
    public class VolunteerMatchingService : IVolunteerMatchingService
    {
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IHelpRequestRepository _helpRequestRepository;
        private readonly ITomTomRoutingService _tomTomRoutingService;

        public VolunteerMatchingService(
            IVolunteerRepository volunteerRepository,
            IHelpRequestRepository helpRequestRepository,
            ITomTomRoutingService tomTomRoutingService)
        {
            _volunteerRepository = volunteerRepository;
            _helpRequestRepository = helpRequestRepository;
            _tomTomRoutingService = tomTomRoutingService;
        }

        public async Task<List<VolunteerMatchResult>> GetEligibleVolunteersAsync(
            int helpRequestId,
            CancellationToken cancellationToken = default)
        {
            // שליפת בקשת העזרה
            var helpRequest =
                _helpRequestRepository.GetById(helpRequestId);

            if (helpRequest == null)
                throw new NotFoundException("Help request not found.");

            if (helpRequest.Status != RequestStatus.Open)
                throw new BusinessRuleException(
                    "Matching is only possible for open requests.");

            if (helpRequest.HelpTypeId == null)
                throw new BusinessRuleException(
                    "The help request must have a help type.");

            // שליפת המתנדבים הפעילים עם סוגי העזרה והציוד שלהם
            var volunteers =
                _volunteerRepository.GetAllWithQualifications();

            // איסוף מזהי הציוד הנדרש
            var requiredEquipmentIds = helpRequest.RequiredEquipments
                .Select(e => e.EquipmentTypeId)
                .Distinct()
                .ToList();

            // סינון לפי תנאי הסף
            var eligibleVolunteers = volunteers
                .Where(volunteer =>
                {
                    // התאמה לסוג העזרה
                    bool hasRequiredHelpType =
                        volunteer.VolunteerHelpTypes.Any(
                            ht => ht.HelpTypeId == helpRequest.HelpTypeId);

                    // החזקת כל הציוד הנדרש
                    bool hasAllRequiredEquipment =
                        requiredEquipmentIds.All(requiredId =>
                            volunteer.VolunteerEquipments.Any(
                                equipment =>
                                    equipment.EquipmentTypeId == requiredId));

                    return hasRequiredHelpType &&
                           hasAllRequiredEquipment;
                })
                .ToList();

            // סינון ראשוני לפי מרחק אווירי
            const double maxAirDistanceKm = 15;

            var nearbyVolunteers = eligibleVolunteers
                .Where(volunteer =>
                {
                    double? distance = CalculateDistanceKm(
                        volunteer,
                        helpRequest);

                    return distance.HasValue &&
                           distance.Value <= maxAirDistanceKm;
                })
                .ToList();

            // אין מתנדבים מתאימים
            if (!nearbyVolunteers.Any())
            {
                return new List<VolunteerMatchResult>();
            }

            // יצירת רשימת מיקומי המתנדבים
            var origins = nearbyVolunteers
                .Select(volunteer => new TomTomLocation(
                    volunteer.Latitude!.Value,
                    volunteer.Longitude!.Value))
                .ToList();

            // מיקום בקשת העזרה
            var destination = new TomTomLocation(
                helpRequest.Latitude,
                helpRequest.Longitude);

            // שליחת כל המתנדבים ל-TomTom בקריאה אחת
            var routes = await _tomTomRoutingService.CalculateRoutesAsync(
                origins,
                destination,
                cancellationToken);

            // שיוך תוצאות המסלול למתנדבים
            var volunteersWithRoutes = routes
                .Where(route =>
                    route.DestinationIndex == 0 &&
                    route.OriginIndex >= 0 &&
                    route.OriginIndex < nearbyVolunteers.Count &&
                    route.TravelTimeInSeconds.HasValue &&
                    string.IsNullOrEmpty(route.ErrorCode))
                .Select(route => new VolunteerMatchResult
                {
                    Volunteer = nearbyVolunteers[route.OriginIndex],
                    TravelTimeInSeconds = route.TravelTimeInSeconds.Value,
                    DistanceInMeters = route.DistanceInMeters,
                    ReliabilityScore =
                        nearbyVolunteers[route.OriginIndex].ReliabilityScore
                })
                .ToList();

            return volunteersWithRoutes;
        }

        // חישוב מרחק אווירי באמצעות נוסחת Haversine
        private static double? CalculateDistanceKm(
            Volunteer volunteer,
            HelpRequest request)
        {
            if (volunteer.Latitude == null ||
                volunteer.Longitude == null)
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
