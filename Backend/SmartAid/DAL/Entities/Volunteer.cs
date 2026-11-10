using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Volunteer
    {
        //טבלת מתנדבים
        //מייצגת את הפרטים והמדדים הסטטיסטיים של המתנדבים
        public int Id { get; set; }


        // קישור לחשבון המשתמש במערכת
        public int UserId { get; set; }
        public User User { get; set; } = null!;


        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // שדות לחישוב מדד אמינות
        public int TotalOffersReceived { get; set; } = 0;
        public int TotalOffersAccepted { get; set; } = 0;
        public int TotalCompleted { get; set; } = 0;
        public double ReliabilityScore { get; set; } = 100.0;

        // Navigation Properties
        public List<VolunteerEquipment> VolunteerEquipments { get; set; } = new();
        public List<HelpRequest> AssignedRequests { get; set; } = new();
        public List<RequestOffer> RequestOffers { get; set; } = new();
    }
}
