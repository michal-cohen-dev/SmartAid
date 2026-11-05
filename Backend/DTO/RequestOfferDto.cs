
using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class RequestOfferDto
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Help request ID must be valid.")]
        public int HelpRequestId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Volunteer ID must be valid.")]
        public int VolunteerId { get; set; }

        [Range(0, 3, ErrorMessage = "Status must be between 0 and 3.")]
        public int Status { get; set; }

        public DateTime OfferedAt { get; set; }
    }
}
