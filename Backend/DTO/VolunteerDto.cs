using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DTO;

public class VolunteerDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Full name must contain between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [StringLength(20, MinimumLength = 9,
        ErrorMessage = "Phone number must contain between 9 and 20 characters.")]
    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public int TotalOffersReceived { get; set; }
    public int TotalOffersAccepted { get; set; }
    public int TotalCompleted { get; set; }
    public double ReliabilityScore { get; set; }
}