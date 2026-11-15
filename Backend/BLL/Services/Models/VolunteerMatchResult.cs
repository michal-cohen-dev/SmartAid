using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace BLL.Services.Models
{
    public class VolunteerMatchResult
    {
        public Volunteer Volunteer { get; set; }

        public int TravelTimeInSeconds { get; set; }

        public int? DistanceInMeters { get; set; }

        public double ReliabilityScore { get; set; }
    }
}