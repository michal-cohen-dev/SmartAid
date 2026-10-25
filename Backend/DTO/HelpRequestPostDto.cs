
using Microsoft.AspNetCore.Http;

namespace DTO
{
    public class HelpRequestPostDto
    {
        public int UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? AddressNotes { get; set; }
        public List<int> RequiredEquipmentTypeIds { get; set; } = new();
        public IFormFile? ImageFile { get; set; }
    }
}
