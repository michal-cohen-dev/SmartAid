using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VolunteerEquipmentDto
    {
        public int Id { get; set; }
        public int VolunteerId { get; set; }
        public int EquipmentTypeId { get; set; }
        public string? EquipmentTypeName { get; set; }
    }
}
