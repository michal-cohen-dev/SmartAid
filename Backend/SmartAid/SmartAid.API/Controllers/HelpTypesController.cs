
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpTypesController : ControllerBase
    {
        private readonly IHelpTypeService _helpTypeService;

        public HelpTypesController(IHelpTypeService helpTypeService)
        {
            _helpTypeService = helpTypeService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var helpTypes = _helpTypeService.GetAll();

            return Ok(helpTypes);
        }
    }
}
