using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public enum OfferStatus
    {
        Pending = 0,
        Accepted = 1,
        Expired = 2,
        Declined = 3
    }

    public class RequestOffer
    {
        //טבלת ניהול הצעות לקריאה
        //מעקב אחר הצעות שנשלחו למתנדבים בזמן אמת
        public int Id { get; set; }

        public int HelpRequestId { get; set; }
        public HelpRequest HelpRequest { get; set; } = null!;

        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; } = null!;

        public OfferStatus Status { get; set; } = OfferStatus.Pending;
        public DateTime OfferedAt { get; set; } = DateTime.UtcNow;
    }
}
