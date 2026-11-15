
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpTypesController : ControllerBase
    {
        private readonly IHelpTypeRepository _helpTypeRepository;

        public HelpTypesController(
            IHelpTypeRepository helpTypeRepository)
        {
            _helpTypeRepository = helpTypeRepository;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var helpTypes = _helpTypeRepository.GetAll()
                .Select(h => new
                {
                    h.Id,
                    h.Name
                })
                .ToList();

            return Ok(helpTypes);
        }
    }
}
