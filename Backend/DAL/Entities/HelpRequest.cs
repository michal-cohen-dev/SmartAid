
using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public enum RequestStatus
    {
        Open = 0,
        Assigned = 1,
        Completed = 2,
        Cancelled = 3,
        InTreatment = 4
    }

    public class HelpRequest
    {
        public int Id { get; set; }

        // FK למשתמש פותח הקריאה
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // כותרת ותיאור של בקשת העזרה
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // מיקום גיאוגרפי
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // כתובת
        public string Address { get; set; } = string.Empty;
        public string? AddressNotes { get; set; }
        public string? ImageName { get; set; }

        public int? HelpTypeId { get; set; }
        public HelpType? HelpType { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Open;

        // המתנדב שהוקצה לקריאה
        public int? AssignedVolunteerId { get; set; }
        public Volunteer? AssignedVolunteer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? Rating { get; set; }

        // Navigation Properties
        public List<RequestEquipment> RequiredEquipments { get; set; } = new();
        public List<RequestOffer> Offers { get; set; } = new();
    }
}
