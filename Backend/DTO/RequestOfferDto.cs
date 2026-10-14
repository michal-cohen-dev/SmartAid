using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class RequestOfferDto
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public int VolunteerId { get; set; }
        public int Status { get; set; }
        public DateTime OfferedAt { get; set; }
    }
}
