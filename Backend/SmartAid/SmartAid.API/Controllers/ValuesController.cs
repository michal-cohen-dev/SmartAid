using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.Interfaces;
using DTO;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerEquipmentsController : ControllerBase
    {
        private readonly IVolunteerEquipmentRepository _repository;

        public VolunteerEquipmentsController(IVolunteerEquipmentRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<VolunteerEquipmentDto>> GetAll() => Ok(_repository.GetAll());

        [HttpGet("{id}")]
        public ActionResult<VolunteerEquipmentDto> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null) return NotFound(new { message = $"VolunteerEquipment with ID {id} not found." });
            return Ok(item);
        }

        [HttpPost]
        public ActionResult Add([FromBody] VolunteerEquipmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _repository.Add(dto);
            return Ok(new { message = "VolunteerEquipment added successfully." });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] VolunteerEquipmentDto dto)
        {
            if (_repository.GetById(id) == null) return NotFound();
            _repository.Update(dto);
            return Ok(new { message = "VolunteerEquipment updated successfully." });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            if (_repository.GetById(id) == null) return NotFound();
            _repository.Delete(id);
            return Ok(new { message = "VolunteerEquipment deleted successfully." });
        }
    }
}
