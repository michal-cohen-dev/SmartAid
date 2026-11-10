using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO;

public class VolunteerEquipmentDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Volunteer ID must be greater than 0.")]
    public int VolunteerId { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Equipment type ID must be greater than 0.")]
    public int EquipmentTypeId { get; set; }

    public string? EquipmentTypeName { get; set; }
}