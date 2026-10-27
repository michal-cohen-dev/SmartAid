
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace DTO
{
    public class HelpRequestPostDto
    {
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty; 
        public string? AddressNotes { get; set; }
        public List<int> RequiredEquipmentTypeIds { get; set; } = new();
        public IFormFile? ImageFile { get; set; }
    }
}
