using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class UpdateVolunteerLocationDto
    {
        [Required]
        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double? Longitude { get; set; }
    }
}
