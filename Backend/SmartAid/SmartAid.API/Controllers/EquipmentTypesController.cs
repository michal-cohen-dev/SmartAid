using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.Interfaces;
using DTO;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentTypesController : ControllerBase
    {
        private readonly IEquipmentTypeRepository _equipmentTypeRepository;

        public EquipmentTypesController(IEquipmentTypeRepository equipmentTypeRepository)
        {
            _equipmentTypeRepository = equipmentTypeRepository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<EquipmentTypeDto>> GetAll() => Ok(_equipmentTypeRepository.GetAll());

        [HttpGet("{id}")]
        public ActionResult<EquipmentTypeDto> GetById(int id)
        {
            var item = _equipmentTypeRepository.GetById(id);
            if (item == null) return NotFound(new { message = $"EquipmentType with ID {id} not found." });
            return Ok(item);
        }

        [HttpPost]
        public ActionResult Add([FromBody] EquipmentTypeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _equipmentTypeRepository.Add(dto);
            return Ok(new { message = "EquipmentType added successfully." });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] EquipmentTypeDto dto)
        {
            if (_equipmentTypeRepository.GetById(id) == null) return NotFound();
            _equipmentTypeRepository.Update(dto);
            return Ok(new { message = "EquipmentType updated successfully." });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            if (_equipmentTypeRepository.GetById(id) == null) return NotFound();
            _equipmentTypeRepository.Delete(id);
            return Ok(new { message = "EquipmentType deleted successfully." });
        }
    }
}
