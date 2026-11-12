
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






        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<HelpRequestDto>> GetAll()
        {
            return Ok(_helpRequestService.GetAll());
        }




        [HttpGet("open")]
        public ActionResult<IEnumerable<HelpRequestPreviewDto>> GetOpenForVolunteer()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            return Ok(_helpRequestService.GetOpenForVolunteer(userId));
        }


        [Authorize]
        [HttpGet("my")]
        public IActionResult GetMyRequests()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var requests = _helpRequestService.GetByUserId(userId);

            return Ok(requests);
        }



        [HttpGet("{id}")]
        public ActionResult<HelpRequestDto> GetById(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                bool isOwner = _helpRequestService.IsOwner(id, userId);
                bool isAssignedVolunteer =
                    _helpRequestService.IsAssignedVolunteer(id, userId);

                if (!isOwner && !isAssignedVolunteer)
                    return NotFound();
            }

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
        [Consumes("multipart/form-data")]
        public ActionResult Add([FromForm] HelpRequestPostDto helpRequestDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // שמירת הבקשה וקבלת המזהה שנוצר
            var requestId = _helpRequestService.Add(helpRequestDto, userId);

            return Ok(new
            {
                message = "Help request added successfully.",
                id = requestId
            });
        }


        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] HelpRequestUpdateDto helpRequestDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var updated = _helpRequestService.Update(id, helpRequestDto, userId);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Help request not found."
                });
            }

            return Ok(new
            {
                message = "Help request updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var deleted = _helpRequestService.Delete(id, userId);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Help request not found."
                });
            }

            return Ok(new
            {
                message = "Help request deleted successfully."
            });
        }


        [HttpPost("{id}/accept")]
        public ActionResult AcceptRequest(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var accepted = _helpRequestService.TryAssignToVolunteer(
                id,
                userId);

            if (!accepted)
            {
                return Conflict(new
                {
                    message = "הבקשה כבר אינה זמינה או שאין לך הרשאה לקבל אותה."
                });
            }

            return Ok(new
            {
                message = "הבקשה הוקצתה אליך בהצלחה."
            });
        }
    }
}
