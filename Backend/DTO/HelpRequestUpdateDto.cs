using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO;

public class HelpRequestUpdateDto
{
    [Range(-90, 90,
        ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Range(-180, 180,
        ErrorMessage = "Longitude must be between -180 and 180.")]
    public double Longitude { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(300, MinimumLength = 2,
        ErrorMessage = "Address must contain between 2 and 300 characters.")]
    public string Address { get; set; } = string.Empty;

    [StringLength(300,
        ErrorMessage = "Address notes cannot exceed 300 characters.")]
    public string? AddressNotes { get; set; }

    [Required(ErrorMessage = "Help type is required.")]
    [Range(1, int.MaxValue,
    ErrorMessage = "A valid help type must be selected.")]
    public int HelpTypeId { get; set; }


    [MaxLength(20,
        ErrorMessage = "You cannot select more than 20 equipment types.")]
    public List<int> RequiredEquipmentTypeIds { get; set; } = new();
}