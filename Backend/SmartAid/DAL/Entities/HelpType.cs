using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAL.Entities
{
    public class HelpType
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<VolunteerHelpType> VolunteerHelpTypes { get; set; } = new();

        public List<HelpRequest> HelpRequests { get; set; } = new();
    }
}
