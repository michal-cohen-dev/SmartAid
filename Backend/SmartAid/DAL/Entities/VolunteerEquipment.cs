using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class VolunteerEquipment
    {
        //טבלת קשר: מתנדב-ציוד
        //קשר רבים לרבים בין מתנדב לציוד שברשותו
        public int Id { get; set; } // מפתח ראשי ייחודי לטבלה

        // Foreign Key למתנדב
        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; } = null!;

        // Foreign Key לסוג הציוד
        public int EquipmentTypeId { get; set; }
        public EquipmentType EquipmentType { get; set; } = null!;
    }
}
