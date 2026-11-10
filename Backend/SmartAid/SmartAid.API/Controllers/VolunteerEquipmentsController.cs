
using Microsoft.AspNetCore.Mvc;
using DTO;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SmartAid.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerEquipmentsController : ControllerBase
    {
        private readonly IVolunteerEquipmentService _service;
        private readonly IVolunteerService _volunteerService;

        public VolunteerEquipmentsController(
            IVolunteerEquipmentService service,
            IVolunteerService volunteerService)
        {
            _service = service;
            _volunteerService = volunteerService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<VolunteerEquipmentDto>> GetAll()
        {
            return Ok(_service.GetAll());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public ActionResult<VolunteerEquipmentDto> GetById(int id)
        {
            var item = _service.GetById(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpGet("my-equipment")]
        public ActionResult<IEnumerable<VolunteerEquipmentDto>> GetMyEquipment()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var volunteer = _volunteerService.GetByUserId(userId);

            if (volunteer == null)
                return NotFound(new
                {
                    message = "Volunteer profile not found."
                });

            var equipment = _service.GetByVolunteerId(volunteer.Id);

            return Ok(equipment);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Add([FromBody] VolunteerEquipmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _service.Add(dto);
            return Ok(new { message = "VolunteerEquipment added successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public ActionResult Update(
            int id,
            [FromBody] VolunteerEquipmentDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data.");

            var existing = _service.GetById(id);

            if (existing == null)
                return NotFound();

            _service.Update(id, dto);

            return Ok(new { message = "VolunteerEquipment updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var existing = _service.GetById(id);

            if (existing == null)
                return NotFound();

            _service.Delete(id);

            return Ok(new { message = "VolunteerEquipment deleted successfully." });
        }
    }
}
