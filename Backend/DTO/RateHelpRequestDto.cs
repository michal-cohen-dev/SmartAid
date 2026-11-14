using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class RateHelpRequestDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
    }
}