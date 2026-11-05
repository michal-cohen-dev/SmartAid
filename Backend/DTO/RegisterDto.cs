using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO;

public class RegisterDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Full name must contain between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(254,
        ErrorMessage = "Email cannot exceed 254 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [StringLength(20, MinimumLength = 9,
        ErrorMessage = "Phone number must contain between 9 and 20 characters.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Password must contain between 8 and 100 characters.")]
    public string Password { get; set; } = string.Empty;
}