using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.Interfaces;
using DTO;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestEquipmentsController : ControllerBase
    {
        private readonly IRequestEquipmentRepository _repository;

        public RequestEquipmentsController(IRequestEquipmentRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<RequestEquipmentDto>> GetAll() => Ok(_repository.GetAll());

        [HttpGet("{id}")]
        public ActionResult<RequestEquipmentDto> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null) return NotFound(new { message = $"RequestEquipment with ID {id} not found." });
            return Ok(item);
        }

        [HttpPost]
        public ActionResult Add([FromBody] RequestEquipmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _repository.Add(dto);
            return Ok(new { message = "RequestEquipment added successfully." });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] RequestEquipmentDto dto)
        {
            if (_repository.GetById(id) == null) return NotFound();
            _repository.Update(dto);
            return Ok(new { message = "RequestEquipment updated successfully." });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            if (_repository.GetById(id) == null) return NotFound();
            _repository.Delete(id);
            return Ok(new { message = "RequestEquipment deleted successfully." });
        }
    }
}
