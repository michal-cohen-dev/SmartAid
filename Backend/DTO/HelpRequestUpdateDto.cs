
using System.ComponentModel.DataAnnotations;

namespace DTO;

public class HelpRequestUpdateDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Title must contain between 2 and 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, MinimumLength = 5,
        ErrorMessage = "Description must contain between 5 and 1000 characters.")]
    public string Description { get; set; } = string.Empty;

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

    [MaxLength(20,
        ErrorMessage = "You cannot select more than 20 equipment types.")]
    public List<int> RequiredEquipmentTypeIds { get; set; } = new();
}
