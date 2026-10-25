
using Microsoft.AspNetCore.Mvc;
using DTO;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace SmartAid.API.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HelpRequestsController : ControllerBase
    {
       private readonly IHelpRequestService _helpRequestService;

        public HelpRequestsController(IHelpRequestService helpRequestService)
        {
            _helpRequestService = helpRequestService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<HelpRequestDto>> GetAll()
        {
            return Ok(_helpRequestService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<HelpRequestDto> GetById(int id)
        {
            var request = _helpRequestService.GetById(id);

            if (request == null)
            {
                return NotFound(new
                {
                    message = $"Help request with ID {id} not found."
                });
            }

            return Ok(request);
        }
        [HttpPost]
        public ActionResult Add([FromBody] HelpRequestPostDto helpRequestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            helpRequestDto.UserId = userId;

            _helpRequestService.Add(helpRequestDto);

            return Ok(new { message = "Help request added successfully." });
        }


        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] HelpRequestDto helpRequestDto)
        {
            if (helpRequestDto == null)
                return BadRequest("Invalid request data.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var updated = _helpRequestService.Update(id, helpRequestDto, userId);

            if (!updated)
                return NotFound(new
                {
                    message = "Help request not found."
                });

            return Ok(new { message = "Help request updated successfully." });
        }
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var deleted = _helpRequestService.Delete(id, userId);

            if (!deleted)
                return NotFound(new
                {
                    message = "Help request not found."
                });

            return Ok(new { message = "Help request deleted successfully." });
        }
    }
}
