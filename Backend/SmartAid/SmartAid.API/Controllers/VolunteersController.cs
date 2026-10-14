using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.Interfaces;
using DAL.Entities;
using DTO;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteersController : ControllerBase
    {
        private readonly IVolunteerRepository _volunteerRepository;

        public VolunteersController(IVolunteerRepository volunteerRepository)
        {
            _volunteerRepository = volunteerRepository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<VolunteerDto>> GetAll()
        {
            var volunteers = _volunteerRepository.GetAll();
            return Ok(volunteers);
        }

        [HttpGet("{id}")]
        public ActionResult<VolunteerDto> GetById(int id)
        {
            var volunteer = _volunteerRepository.GetById(id);
            if (volunteer == null)
            {
                return NotFound(new { message = $"Volunteer with ID {id} not found." });
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

            _volunteerRepository.Add(volunteerDto);
            return Ok(new { message = "Volunteer added successfully." });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] VolunteerDto volunteerDto)
        {
            var existing = _volunteerRepository.GetById(id);
            if (existing == null)
            {
                return NotFound(new { message = $"Volunteer with ID {id} not found." });
            }

            _volunteerRepository.Update(volunteerDto);
            return Ok(new { message = "Volunteer updated successfully." });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _volunteerRepository.GetById(id);
            if (existing == null)
            {
                return NotFound(new { message = $"Volunteer with ID {id} not found." });
            }

            _volunteerRepository.Delete(id);
            return Ok(new { message = "Volunteer deleted successfully." });
        }
    }
}
