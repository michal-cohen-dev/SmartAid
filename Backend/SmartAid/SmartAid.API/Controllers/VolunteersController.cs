using Microsoft.AspNetCore.Mvc;
using DTO;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SmartAid.API.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<VolunteerDto>> GetAll()
        {
            return Ok(_volunteerService.GetAll());
        }

        [Authorize(Roles = "Admin")]
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
        public ActionResult Add(
            [FromBody] RegisterVolunteerDto volunteerDto)
        {
            var currentUserId = User.GetUserId();

            if (currentUserId == null)
                return Unauthorized();

            _volunteerService.Add(
                volunteerDto,
                currentUserId.Value);

            return Ok(new
            {
                message = "Volunteer added successfully."
            });
        }

        [HttpPut("{id}")]
        public ActionResult Update(
            int id,
            [FromBody] VolunteerDto volunteerDto)
        {
            var currentUserId = User.GetUserId();

            if (currentUserId == null)
                return Unauthorized();

            _volunteerService.Update(
                id,
                volunteerDto,
                currentUserId.Value);

            return Ok(new
            {
                message = "Volunteer updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var currentUserId = User.GetUserId();

            if (currentUserId == null)
                return Unauthorized();

            _volunteerService.Delete(
                id,
                currentUserId.Value);

            return Ok(new
            {
                message = "Volunteer deleted successfully."
            });
        }

        [HttpGet("me")]
        public ActionResult<VolunteerDto> GetMyProfile()
        {
            var currentUserId = User.GetUserId();

            if (currentUserId == null)
                return Unauthorized();

            var volunteer =
                _volunteerService.GetByUserId(currentUserId.Value);

            if (volunteer == null)
                return NotFound(new
                {
                    message = "Volunteer profile not found for the current user."
                });

            return Ok(volunteer);
        }

        [HttpPut("me/location")]
        public ActionResult UpdateMyLocation(
            [FromBody] UpdateVolunteerLocationDto locationDto)
        {
            var currentUserId = User.GetUserId();

            if (currentUserId == null)
                return Unauthorized();

            _volunteerService.UpdateLocation(
                currentUserId.Value,
                locationDto.Latitude!.Value,
                locationDto.Longitude!.Value);

            return Ok(new
            {
                message = "Volunteer location updated successfully."
            });
        }

        [HttpPut("me/availability")]
        public async Task<ActionResult> UpdateMyAvailability(
            [FromBody] UpdateVolunteerAvailabilityDto availabilityDto,
            CancellationToken cancellationToken)
        {
            var currentUserId = User.GetUserId();

            if (currentUserId == null)
                return Unauthorized();

            await _volunteerService.UpdateAvailabilityAsync(
                currentUserId.Value,
                availabilityDto.IsAvailable,
                cancellationToken);

            return Ok(new
            {
                message = "Volunteer availability updated successfully."
            });
        }
    }
}