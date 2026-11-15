
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO;

public class RegisterVolunteerDto
{
    public List<int> EquipmentTypeIds { get; set; } = new();
    public List<int> HelpTypeIds { get; set; } = new();

    [Required]
    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double? Longitude { get; set; }
}
