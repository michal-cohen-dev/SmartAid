using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class EquipmentType
    {
        //טבלת סוגי ציוד
        //סוגי הציוד הזמינים במערכת
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation Properties
        public List<VolunteerEquipment> VolunteerEquipments { get; set; } = new();
        public List<RequestEquipment> RequestEquipments { get; set; } = new();
    }
}
