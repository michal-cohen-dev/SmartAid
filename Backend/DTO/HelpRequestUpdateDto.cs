using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO;

public class HelpRequestUpdateDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? AddressNotes { get; set; }
    public List<int> RequiredEquipmentTypeIds { get; set; } = new();
}