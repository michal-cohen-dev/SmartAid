
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
    public class RequestOffersController : ControllerBase
    {
        private readonly IRequestOfferService _requestOfferService;

        public RequestOffersController(IRequestOfferService requestOfferService)
        {
            _requestOfferService = requestOfferService;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<RequestOfferDto>> GetAll()
        {
            return Ok(_requestOfferService.GetAll());
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public ActionResult<RequestOfferDto> GetById(int id)
        {
            var offer = _requestOfferService.GetById(id);

            if (offer == null)
            {
                return NotFound(new
                {
                    message = $"Request offer with ID {id} not found."
                });
            }

            return Ok(offer);
        }

        [HttpPost]
        public ActionResult Add([FromBody] RequestOfferDto requestOfferDto)
        {
            return StatusCode(403, new
            {
                message = "Request offers cannot be created manually."
            });
        }


        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] RequestOfferDto requestOfferDto)
        {
            return StatusCode(403, new
            {
                message = "Request offers cannot be updated manually."
            });
        }



        [HttpPost("{id}/decline")]
        public ActionResult DeclineOffer(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var declined = _requestOfferService.TryDeclineOffer(
                id,
                userId);

            if (!declined)
            {
                return Conflict(new
                {
                    message = "לא ניתן לדחות את ההצעה. ייתכן שהיא כבר טופלה או שאינה שייכת לך."
                });
            }

            return Ok(new
            {
                message = "ההצעה נדחתה בהצלחה."
            });
        }



        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            return StatusCode(403, new
            {
                message = "Request offers cannot be deleted manually."
            });
        }

    }
}
