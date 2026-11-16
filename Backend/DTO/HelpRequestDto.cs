
using System;
using System.Collections.Generic;

namespace DTO
{
    public class HelpRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? UserFullName { get; set; }
        public string? ImageName { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? AddressNotes { get; set; }

        public int Status { get; set; }
        public int? AssignedVolunteerId { get; set; }
        public string? AssignedVolunteerName { get; set; }

        public DateTime CreatedAt { get; set; }
        public int? Rating { get; set; }

        public double? AssignedVolunteerLatitude { get; set; }
        public double? AssignedVolunteerLongitude { get; set; }
        public DateTime? AssignedVolunteerLastLocationUpdatedAt { get; set; }

        // Estimated arrival time in seconds
        public int? EstimatedArrivalSeconds { get; set; }

        public List<int> RequiredEquipmentTypeIds { get; set; } = new();
        public List<string> RequiredEquipmentNames { get; set; } = new();
    }
}
