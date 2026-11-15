using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace BLL.Services.Models
{
    public class HelpRequestMatchResult
    {
        public HelpRequest HelpRequest { get; set; }

        public int TravelTimeInSeconds { get; set; }

        public int? DistanceInMeters { get; set; }

        public double ReliabilityScore { get; set; }

        public double MatchScore { get; set; }
    }
}