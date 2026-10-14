using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.Interfaces;
using DTO;

namespace SmartAid.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> GetAll()
        {
            return Ok(_userRepository.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<UserDto> GetById(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
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

            _userRepository.Add(userDto);
            return Ok(new { message = "User added successfully." });
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] UserDto userDto)
        {
            var existing = _userRepository.GetById(id);
            if (existing == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
            }

            _userRepository.Update(userDto);
            return Ok(new { message = "User updated successfully." });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _userRepository.GetById(id);
            if (existing == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
            }

            _userRepository.Delete(id);
            return Ok(new { message = "User deleted successfully." });
        }
    }
}
