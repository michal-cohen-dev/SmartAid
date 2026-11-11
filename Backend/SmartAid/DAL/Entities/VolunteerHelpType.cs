using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAL.Entities
{
    public class VolunteerHelpType
    {
        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; } = null!;

        public int HelpTypeId { get; set; }
        public HelpType HelpType { get; set; } = null!;
    }
}
