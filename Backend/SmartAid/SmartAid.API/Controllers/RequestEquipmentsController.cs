
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using DTO;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestEquipmentsController : ControllerBase
    {
        private readonly IRequestEquipmentService _service;

        public RequestEquipmentsController(IRequestEquipmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<RequestEquipmentDto>> GetAll()
        {
            return Ok(_service.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<RequestEquipmentDto> GetById(int id)
        {
            var item = _service.GetById(id);

            if (item == null)
            {
                return NotFound(new
                {
                    message = $"RequestEquipment with ID {id} not found."
                });
            }

            return Ok(item);
        }

        [HttpPost]
        public ActionResult Add([FromBody] RequestEquipmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _service.Add(dto);

            return Ok(new { message = "RequestEquipment added successfully." });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] RequestEquipmentDto dto)
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

            return Ok(new { message = "RequestEquipment updated successfully." });
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

            return Ok(new { message = "RequestEquipment deleted successfully." });
        }
    }
}
