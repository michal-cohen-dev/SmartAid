
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using DTO;

namespace SmartAid.API.Controllers
{
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
        public ActionResult Add([FromForm] HelpRequestPostDto helpRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _helpRequestService.Add(helpRequestDto);
                return Ok(new { message = "Help request added successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] HelpRequestDto helpRequestDto)
        {
            if (helpRequestDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            var existing = _helpRequestService.GetById(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    message = $"Help request with ID {id} not found."
                });
            }

            _helpRequestService.Update(id, helpRequestDto);

            return Ok(new { message = "Help request updated successfully." });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _helpRequestService.GetById(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    message = $"Help request with ID {id} not found."
                });
            }

            _helpRequestService.Delete(id);

            return Ok(new { message = "Help request deleted successfully." });
        }
    }
}
