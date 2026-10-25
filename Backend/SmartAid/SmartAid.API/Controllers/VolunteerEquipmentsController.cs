
using Microsoft.AspNetCore.Mvc;
using DTO;
using BLL.Services.Interfaces;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerEquipmentsController : ControllerBase
    {
        private readonly IVolunteerEquipmentService _service;

        public VolunteerEquipmentsController(IVolunteerEquipmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<VolunteerEquipmentDto>> GetAll()
        {
            return Ok(_service.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<VolunteerEquipmentDto> GetById(int id)
        {
            var item = _service.GetById(id);

            if (item == null)
            {
                return NotFound(new
                {
                    message = $"VolunteerEquipment with ID {id} not found."
                });
            }

            return Ok(item);
        }

        [HttpPost]
        public ActionResult Add([FromBody] VolunteerEquipmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _service.Add(dto);

            return Ok(new
            {
                message = "VolunteerEquipment added successfully."
            });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] VolunteerEquipmentDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid request data.");
            }

            var existing = _service.GetById(id);

            if (existing == null)
            {
                return NotFound();
            }

            _service.Update(id, dto);

            return Ok(new
            {
                message = "VolunteerEquipment updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _service.GetById(id);

            if (existing == null)
            {
                return NotFound();
            }

            _service.Delete(id);

            return Ok(new
            {
                message = "VolunteerEquipment deleted successfully."
            });
        }
    }
}
