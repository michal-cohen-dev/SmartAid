
using Microsoft.AspNetCore.Mvc;
using DTO;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;

namespace SmartAid.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteersController : ControllerBase
    {
        private readonly IVolunteerService _volunteerService;

        public VolunteersController(IVolunteerService volunteerService)
        {
            _volunteerService = volunteerService;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out var currentUserId))
                return currentUserId;

            return null;
        }


        [Authorize(Roles = "Admin")]
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

            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized();
            }

            try
            {
                _volunteerService.Add(volunteerDto, currentUserId.Value);

                return Ok(new
                {
                    message = "Volunteer added successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] VolunteerDto volunteerDto)
        {
            if (volunteerDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized();
            }

            try
            {
                _volunteerService.Update(id, volunteerDto, currentUserId.Value);

                return Ok(new
                {
                    message = "Volunteer updated successfully."
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    message = "Volunteer profile not found."
                });
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized();
            }

            try
            {
                _volunteerService.Delete(id, currentUserId.Value);

                return Ok(new
                {
                    message = "Volunteer deleted successfully."
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    message = "Volunteer profile not found."
                });
            }
        }
    }
}
