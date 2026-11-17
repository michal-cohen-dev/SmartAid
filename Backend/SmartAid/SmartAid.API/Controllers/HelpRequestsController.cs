using Microsoft.AspNetCore.Mvc;
using DTO;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using System.Threading.Tasks;
using SmartAid.API.Services;
using Microsoft.AspNetCore.SignalR;
using SmartAid.API.Hubs;
using SmartAid.API.Extensions;

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
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var requests = await _helpRequestService.GetOpenForVolunteerAsync(
                userId.Value,
                cancellationToken);

            return Ok(requests);
        }

        [HttpGet("my")]
        public IActionResult GetMyRequests()
        {
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            return Ok(_helpRequestService.GetByUserId(userId.Value));
        }

        [HttpGet("volunteer/my")]
        public IActionResult GetMyVolunteerRequests()
        {
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            return Ok(_helpRequestService.GetByVolunteerId(userId.Value));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HelpRequestDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            if (!User.IsInRole("Admin"))
            {
                var userId = User.GetUserId();

                if (userId == null)
                    return Unauthorized();

                bool isOwner =
                    _helpRequestService.IsOwner(id, userId.Value);

                bool isAssignedVolunteer =
                    _helpRequestService.IsAssignedVolunteer(id, userId.Value);

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
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var requestId = await _helpRequestService.Add(
                helpRequestDto,
                userId.Value,
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
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var updated = _helpRequestService.Update(
                id,
                helpRequestDto,
                userId.Value);

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



        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/admin-cancel")]
        public async Task<IActionResult> AdminCancel(int id)
        {
            var requestBeforeCancel = _helpRequestService.GetById(id);

            if (requestBeforeCancel == null)
            {
                return NotFound(new
                {
                    message = "הקריאה לא נמצאה."
                });
            }

            bool cancelled =
                _helpRequestService.AdminCancelRequest(id);

            if (!cancelled)
            {
                return Conflict(new
                {
                    message = "לא ניתן לבטל קריאה שכבר הושלמה או בוטלה."
                });
            }

            await NotifyRequestUpdatedAsync(
                id,
                requestBeforeCancel.UserId,
                requestBeforeCancel.AssignedVolunteerId);

            return Ok(new
            {
                message = "הקריאה בוטלה בהצלחה על ידי המנהל."
            });
        }




        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var requestBeforeDelete = _helpRequestService.GetById(id);

            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var deleted = _helpRequestService.Delete(id, userId.Value);

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
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var accepted = await _helpRequestService.TryAssignToVolunteerAsync(
     id,
     userId.Value,
     HttpContext.RequestAborted);

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
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var marked = _helpRequestService.TryMarkInTreatment(
                id,
                userId.Value);

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
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var completed = _helpRequestService.TryCompleteRequest(
                id,
                userId.Value);

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
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var rated = _helpRequestService.TryRateCompletedRequest(
                id,
                userId.Value,
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
        public async Task<IActionResult> Cancel(
     int id,
     CancellationToken cancellationToken)
        {
            var requestBeforeCancel = _helpRequestService.GetById(id);

            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            bool cancelled = await _helpRequestService.TryCancelRequestAsync(
                id,
                userId.Value,
                cancellationToken);

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