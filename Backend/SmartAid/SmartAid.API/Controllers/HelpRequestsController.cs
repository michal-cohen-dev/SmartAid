using BLL.Repositories.Interfaces;
using DAL.Entities;
using DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpRequestsController : ControllerBase
    {
        private readonly IHelpRequestRepository _helpRequestRepository;

        public HelpRequestsController(IHelpRequestRepository helpRequestRepository)
        {
            _helpRequestRepository = helpRequestRepository;
        }

        // 1. GET: שליפת רשימת כל בקשות העזרה
        [HttpGet]
        public ActionResult<IEnumerable<HelpRequestDto>> GetAll()
        {
            var requests = _helpRequestRepository.GetAll();
            return Ok(requests);
        }

        // 2. GET לפי ID: שליפת בקשת עזרה ספציפית לפי מזהה
        [HttpGet("{id}")]
        public ActionResult<HelpRequestDto> GetById(int id)
        {
            var request = _helpRequestRepository.GetById(id);
            if (request == null)
            {
                return NotFound(new { message = $"Help request with ID {id} not found." });
            }
            return Ok(request);
        }

        // 3. POST: הוספת בקשת עזרה חדשה
        [HttpPost]
        public ActionResult Add([FromBody] HelpRequestDto helpRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _helpRequestRepository.Add(helpRequestDto);
            return Ok(new { message = "Help request added successfully." });
        }

        // 4. PUT: עדכון בקשת עזרה קיימת (רשות / מומלץ מאוד)
        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] HelpRequestDto helpRequestDto)
        {
            if (helpRequestDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            // וידוא שהאובייקט קיים לפני עדכון
            var existing = _helpRequestRepository.GetById(id);
            if (existing == null)
            {
                return NotFound(new { message = $"Help request with ID {id} not found." });
            }

            _helpRequestRepository.Update(helpRequestDto);
            return Ok(new { message = "Help request updated successfully." });
        }

        // 5. DELETE: מחיקת בקשת עזרה לפי מזהה (רשות / מומלץ מאוד)
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _helpRequestRepository.GetById(id);
            if (existing == null)
            {
                return NotFound(new { message = $"Help request with ID {id} not found." });
            }

            _helpRequestRepository.Delete(id);
            return Ok(new { message = "Help request deleted successfully." });
        }
    }
}
