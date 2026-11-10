using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public enum RequestStatus
    {
        Open = 0,
        Assigned = 1,
        Completed = 2,
        Cancelled = 3
    }

    public class HelpRequest
    {
        public int Id { get; set; }

        // FK למשתמש פותח הקריאה
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // מיקום גיאוגרפי עבור אלגוריתם המרחק והניווט
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // תיאור מיקום טקסטואלי לצפייה על ידי מתנדבים ומשתמשים
        public string Address { get; set; } = string.Empty;

        // הערות מיקום נוספות (למשל: קומה/דירה, או תיאור השוליים בכביש)
        public string? AddressNotes { get; set; }
        public string? ImageName { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Open;

        // FK למתנדב שתפס את הקריאה (Nullable)
        public int? AssignedVolunteerId { get; set; }
        public Volunteer? AssignedVolunteer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? Rating { get; set; } // דירוג שניתן בסיום הקריאה (1-5)

        // Navigation Properties
        public List<RequestEquipment> RequiredEquipments { get; set; } = new();
        public List<RequestOffer> Offers { get; set; } = new();
    }
}