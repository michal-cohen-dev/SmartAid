using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VolunteerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalOffersReceived { get; set; }
        public int TotalOffersAccepted { get; set; }
        public int TotalCompleted { get; set; }
        public double ReliabilityScore { get; set; }
    }
}
