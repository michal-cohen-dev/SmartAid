using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DTO;

public class EquipmentTypeDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Equipment type name is required.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Equipment type name must contain between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;
}