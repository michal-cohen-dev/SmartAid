
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using DTO;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> GetAll()
        {
            return Ok(_userService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<UserDto> GetById(int id)
        {
            var user = _userService.GetById(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = $"User with ID {id} not found."
                });
            }

            return Ok(user);
        }

        [HttpPost]
        public ActionResult Add([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _userService.Add(userDto);

            return Ok(new
            {
                message = "User added successfully."
            });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] UserDto userDto)
        {
            var existing = _userService.GetById(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    message = $"User with ID {id} not found."
                });
            }

            _userService.Update(id, userDto);

            return Ok(new
            {
                message = "User updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _userService.GetById(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    message = $"User with ID {id} not found."
                });
            }

            _userService.Delete(id);

            return Ok(new
            {
                message = "User deleted successfully."
            });
        }
    }
}
