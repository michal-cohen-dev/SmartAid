using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.Interfaces;
using DTO;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestOffersController : ControllerBase
    {
        private readonly IRequestOfferRepository _requestOfferRepository;

        public RequestOffersController(IRequestOfferRepository requestOfferRepository)
        {
            _requestOfferRepository = requestOfferRepository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<RequestOfferDto>> GetAll()
        {
            return Ok(_requestOfferRepository.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<RequestOfferDto> GetById(int id)
        {
            var offer = _requestOfferRepository.GetById(id);
            if (offer == null)
            {
                return NotFound(new { message = $"Request offer with ID {id} not found." });
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

            _requestOfferRepository.Add(requestOfferDto);
            return Ok(new { message = "Request offer added successfully." });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] RequestOfferDto requestOfferDto)
        {
            var existing = _requestOfferRepository.GetById(id);
            if (existing == null)
            {
                return NotFound(new { message = $"Request offer with ID {id} not found." });
            }

            _requestOfferRepository.Update(requestOfferDto);
            return Ok(new { message = "Request offer updated successfully." });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _requestOfferRepository.GetById(id);
            if (existing == null)
            {
                return NotFound(new { message = $"Request offer with ID {id} not found." });
            }

            _requestOfferRepository.Delete(id);
            return Ok(new { message = "Request offer deleted successfully." });
        }
    }
}
