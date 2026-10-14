using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class RequestEquipmentDto
    {
        public int Id { get; set; }
        public int HelpRequestId { get; set; }
        public int EquipmentTypeId { get; set; }
        public int Quantity { get; set; }
    }
}
