
using BLL.Services.Interfaces;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpTypesController : ControllerBase
    {
        private readonly IHelpTypeService _helpTypeService;

        public HelpTypesController(IHelpTypeService helpTypeService)
        {
            _helpTypeService = helpTypeService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<HelpTypeDto>> GetAll()
        {
            return Ok(_helpTypeService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<HelpTypeDto> GetById(int id)
        {
            var helpType = _helpTypeService.GetById(id);

            if (helpType == null)
                return NotFound();

            return Ok(helpType);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Add([FromBody] HelpTypeDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data.");

            _helpTypeService.Add(dto);

            return Ok(new
            {
                message = "Help type added successfully."
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] HelpTypeDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data.");

            _helpTypeService.Update(id, dto);

            return Ok(new
            {
                message = "Help type updated successfully."
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _helpTypeService.Delete(id);

            return Ok(new
            {
                message = "Help type deleted successfully."
            });
        }
    }
}
