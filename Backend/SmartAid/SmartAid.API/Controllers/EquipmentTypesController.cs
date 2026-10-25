
using Microsoft.AspNetCore.Mvc;
using DTO;
using BLL.Services.Interfaces;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentTypesController : ControllerBase
    {
        private readonly IEquipmentTypeService _equipmentTypeService;

        public EquipmentTypesController(IEquipmentTypeService equipmentTypeService)
        {
            _equipmentTypeService = equipmentTypeService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<EquipmentTypeDto>> GetAll()
        {
            return Ok(_equipmentTypeService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<EquipmentTypeDto> GetById(int id)
        {
            var equipmentType = _equipmentTypeService.GetById(id);

            if (equipmentType == null)
            {
                return NotFound(new
                {
                    message = $"EquipmentType with ID {id} not found."
                });
            }

            return Ok(equipmentType);
        }

        [HttpPost]
        public ActionResult Add([FromBody] EquipmentTypeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _equipmentTypeService.Add(dto);

            return Ok(new
            {
                message = "EquipmentType added successfully."
            });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] EquipmentTypeDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid request data.");
            }

            var existing = _equipmentTypeService.GetById(id);

            if (existing == null)
            {
                return NotFound();
            }

            _equipmentTypeService.Update(id, dto);

            return Ok(new
            {
                message = "EquipmentType updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _equipmentTypeService.GetById(id);

            if (existing == null)
            {
                return NotFound();
            }

            _equipmentTypeService.Delete(id);

            return Ok(new
            {
                message = "EquipmentType deleted successfully."
            });
        }
    }
}
