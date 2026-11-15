using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DTO;

public class RegisterVolunteerDto
{
    public List<int> EquipmentTypeIds { get; set; } = new();
    public List<int> HelpTypeIds { get; set; } = new();
}