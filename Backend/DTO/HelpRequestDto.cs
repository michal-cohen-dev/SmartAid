using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class HelpRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserFullName { get; set; } // לעזרת הצגה נוחה בקליינט
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? AddressNotes { get; set; }
        public int Status { get; set; } // אפשר להעביר כint או כEnum תואם
        public int? AssignedVolunteerId { get; set; }
        public string? AssignedVolunteerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? Rating { get; set; }

        public List<int> RequiredEquipmentTypeIds { get; set; } = new();
    }
}
