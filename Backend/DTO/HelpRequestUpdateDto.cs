
using System.ComponentModel.DataAnnotations;

namespace DTO;

public class HelpRequestUpdateDto
{
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double Longitude { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [StringLength(500)]
    public string? AddressNotes { get; set; }

    public List<int> RequiredEquipmentTypeIds { get; set; } = new();
}
