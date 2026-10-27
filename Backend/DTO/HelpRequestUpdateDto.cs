using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DTO;

public class HelpRequestUpdateDto
{
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    public string Address { get; set; } = string.Empty;
    public string? AddressNotes { get; set; }
    public List<int> RequiredEquipmentTypeIds { get; set; } = new();
}