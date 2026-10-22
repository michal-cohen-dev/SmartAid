
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using DTO;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteersController : ControllerBase
    {
        private readonly IVolunteerService _volunteerService;

        public VolunteersController(IVolunteerService volunteerService)
        {
            _volunteerService = volunteerService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<VolunteerDto>> GetAll()
        {
            return Ok(_volunteerService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<VolunteerDto> GetById(int id)
        {
            var volunteer = _volunteerService.GetById(id);

            if (volunteer == null)
            {
                return NotFound(new
                {
                    message = $"Volunteer with ID {id} not found."
                });
            }

            return Ok(volunteer);
        }

        [HttpPost]
        public ActionResult Add([FromBody] VolunteerDto volunteerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _volunteerService.Add(volunteerDto);

            return Ok(new
            {
                message = "Volunteer added successfully."
            });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] VolunteerDto volunteerDto)
        {
            if (volunteerDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            var existing = _volunteerService.GetById(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    message = $"Volunteer with ID {id} not found."
                });
            }

            _volunteerService.Update(id, volunteerDto);

            return Ok(new
            {
                message = "Volunteer updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _volunteerService.GetById(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    message = $"Volunteer with ID {id} not found."
                });
            }

            _volunteerService.Delete(id);

            return Ok(new
            {
                message = "Volunteer deleted successfully."
            });
        }
    }
}
