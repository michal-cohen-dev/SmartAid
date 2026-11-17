using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class RequestEquipment
    {
        //טבלת קישור: ציוד בקריאה
        //קשר המגדיר אילו פריטי ציוד נדרשים עבור קריאה מסוימת
        public int Id { get; set; }

        public int HelpRequestId { get; set; }
        public HelpRequest HelpRequest { get; set; } = null!;

        public int EquipmentTypeId { get; set; }
        public EquipmentType EquipmentType { get; set; } = null!;
    }
}
