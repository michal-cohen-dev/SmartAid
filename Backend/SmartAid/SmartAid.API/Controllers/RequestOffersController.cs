
using Microsoft.AspNetCore.Mvc;
using DTO;
using BLL.Services.Interfaces;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestOffersController : ControllerBase
    {
        private readonly IRequestOfferService _requestOfferService;

        public RequestOffersController(IRequestOfferService requestOfferService)
        {
            _requestOfferService = requestOfferService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<RequestOfferDto>> GetAll()
        {
            return Ok(_requestOfferService.GetAll());
        }

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _requestOfferService.Add(requestOfferDto);

            return Ok(new
            {
                message = "Request offer added successfully."
            });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] RequestOfferDto requestOfferDto)
        {
            if (requestOfferDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            var existing = _requestOfferService.GetById(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    message = $"Request offer with ID {id} not found."
                });
            }

            _requestOfferService.Update(id, requestOfferDto);

            return Ok(new
            {
                message = "Request offer updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _requestOfferService.GetById(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    message = $"Request offer with ID {id} not found."
                });
            }

            _requestOfferService.Delete(id);

            return Ok(new
            {
                message = "Request offer deleted successfully."
            });
        }
    }
}
