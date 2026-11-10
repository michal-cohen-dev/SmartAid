
using Microsoft.AspNetCore.Mvc;
using DTO;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

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
