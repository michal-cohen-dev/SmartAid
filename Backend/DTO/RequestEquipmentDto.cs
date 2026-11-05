using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO;

public class RequestEquipmentDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Help request ID must be greater than 0.")]
    public int HelpRequestId { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Equipment type ID must be greater than 0.")]
    public int EquipmentTypeId { get; set; }
}