
using Microsoft.AspNetCore.Mvc;
using DTO;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using SmartAid.API.Services;
using Microsoft.AspNetCore.SignalR;
using SmartAid.API.Hubs;

namespace SmartAid.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HelpRequestsController : ControllerBase
    {
        private readonly IHelpRequestService _helpRequestService;
        private readonly EstimatedArrivalService _estimatedArrivalService;
        private readonly IHubContext<NotificationsHub> _hubContext;

        public HelpRequestsController(
            IHelpRequestService helpRequestService,
            EstimatedArrivalService estimatedArrivalService,
            IHubContext<NotificationsHub> hubContext)
        {
            _helpRequestService = helpRequestService;
            _estimatedArrivalService = estimatedArrivalService;
            _hubContext = hubContext;
        }

        private async Task NotifyRequestUpdatedAsync(
            int requestId,
            int? ownerUserId = null,
            int? volunteerId = null)
        {
            var request = _helpRequestService.GetById(requestId);

            var requesterId = ownerUserId ?? request?.UserId;
            var assignedVolunteerId =
                volunteerId ?? request?.AssignedVolunteerId;

            var notification = new
            {
                HelpRequestId = requestId,
                Status = request?.Status
            };

            if (requesterId.HasValue)
            {
                await _hubContext.Clients
                    .Group($"user:{requesterId.Value}")
                    .SendAsync("RequestUpdated", notification);
            }

            if (assignedVolunteerId.HasValue)
            {
                await _hubContext.Clients
                    .Group($"volunteer:{assignedVolunteerId.Value}")
                    .SendAsync("RequestUpdated", notification);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<HelpRequestDto>> GetAll()
        {
            return Ok(_helpRequestService.GetAll());
        }

        [HttpGet("open")]
        public async Task<ActionResult<IEnumerable<HelpRequestPreviewDto>>> GetOpenForVolunteer(
            CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var requests = await _helpRequestService.GetOpenForVolunteerAsync(
                userId,
                cancellationToken);

            return Ok(requests);
        }

        [HttpGet("my")]
        public IActionResult GetMyRequests()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            return Ok(_helpRequestService.GetByUserId(userId));
        }

        [HttpGet("volunteer/my")]
        public IActionResult GetMyVolunteerRequests()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            return Ok(_helpRequestService.GetByVolunteerId(userId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HelpRequestDto>> GetById(
            int id,
            CancellationToken cancellationToken)
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

            request.EstimatedArrivalSeconds =
                await _estimatedArrivalService.GetEstimatedArrivalSecondsAsync(
                    request,
                    cancellationToken);

            return Ok(request);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> Add(
            [FromForm] HelpRequestPostDto helpRequestDto,
            CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var requestId = await _helpRequestService.Add(
                helpRequestDto,
                userId,
                cancellationToken);

            return Ok(new
            {
                message = "Help request added successfully.",
                id = requestId
            });
        }

        [HttpPut("{id}")]
        public ActionResult Update(
            int id,
            [FromBody] HelpRequestUpdateDto helpRequestDto)
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
        public async Task<ActionResult> Delete(int id)
        {
            var requestBeforeDelete = _helpRequestService.GetById(id);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var deleted = _helpRequestService.Delete(id, userId);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Request not found."
                });
            }

            await NotifyRequestUpdatedAsync(
                id,
                requestBeforeDelete?.UserId,
                requestBeforeDelete?.AssignedVolunteerId);

            return Ok(new
            {
                message = "Help request cancelled successfully."
            });
        }

        [HttpPost("{id}/accept")]
        public async Task<ActionResult> AcceptRequest(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var accepted = _helpRequestService.TryAssignToVolunteer(id, userId);

            if (!accepted)
            {
                return Conflict(new
                {
                    message = "הבקשה כבר אינה זמינה או שאין לך הרשאה לקבל אותה."
                });
            }

            await NotifyRequestUpdatedAsync(id);

            return Ok(new
            {
                message = "הבקשה הוקצתה אליך בהצלחה."
            });
        }

        [HttpPost("{id}/arrive")]
        public async Task<ActionResult> MarkArrived(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var marked = _helpRequestService.TryMarkInTreatment(id, userId);

            if (!marked)
            {
                return Conflict(new
                {
                    message = "לא ניתן לסמן הגעה. ייתכן שהבקשה אינה משויכת אליך או שכבר שונה הסטטוס שלה."
                });
            }

            await NotifyRequestUpdatedAsync(id);

            return Ok(new
            {
                message = "הגעתך סומנה בהצלחה."
            });
        }

        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteRequest(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var completed = _helpRequestService.TryCompleteRequest(id, userId);

            if (!completed)
            {
                return Conflict(new
                {
                    message = "לא ניתן לסיים את הטיפול. ייתכן שהבקשה אינה משויכת אליך או שעדיין לא סומנה הגעה."
                });
            }

            await NotifyRequestUpdatedAsync(id);

            return Ok(new
            {
                message = "הטיפול הסתיים בהצלחה."
            });
        }

        [HttpPost("{id}/rate")]
        public ActionResult RateRequest(
            int id,
            [FromBody] RateHelpRequestDto ratingDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var rated = _helpRequestService.TryRateCompletedRequest(
                id,
                userId,
                ratingDto.Rating);

            if (!rated)
            {
                return Conflict(new
                {
                    message = "לא ניתן לדרג את הבקשה. ייתכן שהיא לא הושלמה, אינה שייכת לך או שכבר דורגה."
                });
            }

            return Ok(new
            {
                message = "הדירוג נשמר בהצלחה."
            });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var requestBeforeCancel = _helpRequestService.GetById(id);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            bool cancelled = _helpRequestService.TryCancelRequest(id, userId);

            if (!cancelled)
            {
                return BadRequest(
                    "לא ניתן לבטל את הבקשה. ייתכן שהיא כבר בוטלה או שהמשתמש אינו מורשה לבטל אותה.");
            }

            await NotifyRequestUpdatedAsync(
                id,
                requestBeforeCancel?.UserId,
                requestBeforeCancel?.AssignedVolunteerId);

            return Ok("הבקשה בוטלה בהצלחה.");
        }
    }
}
