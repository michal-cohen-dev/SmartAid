
using Microsoft.AspNetCore.Mvc;
using DTO;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using SmartAid.API.Extensions;

namespace SmartAid.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // פעולות משתמש רגיל

        [HttpGet("{id:int}")]
        public ActionResult<UserDto> GetById(int id)
        {
            var currentUserId = User.GetUserId();

            if (currentUserId == null)
                return Unauthorized();

            var user = _userService.GetById(
                id,
                currentUserId.Value);

            return Ok(user);
        }

        [HttpPut("{id:int}")]
        public ActionResult Update(
            int id,
            [FromBody] UserUpdateDto userDto)
        {
            var currentUserId = User.GetUserId();

            if (currentUserId == null)
                return Unauthorized();

            _userService.Update(
                id,
                userDto,
                currentUserId.Value);

            return Ok(new
            {
                message = "User updated successfully."
            });
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var currentUserId = User.GetUserId();

            if (currentUserId == null)
                return Unauthorized();

            _userService.Delete(
                id,
                currentUserId.Value);

            return Ok(new
            {
                message = "User deleted successfully."
            });
        }

        // פעולות מנהל בלבד

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public ActionResult<List<UserDto>> GetAllForAdmin()
        {
            var users = _userService.GetAllForAdmin();

            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/{id:int}")]
        public ActionResult<UserDto> GetByIdForAdmin(int id)
        {
            var user = _userService.GetByIdForAdmin(id);

            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("admin/{id:int}")]
        public ActionResult UpdateForAdmin(
            int id,
            [FromBody] UserUpdateDto userDto)
        {
            _userService.UpdateForAdmin(id, userDto);

            return Ok(new
            {
                message = "User updated successfully."
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id:int}")]
        public ActionResult DeleteForAdmin(int id)
        {
            _userService.DeleteForAdmin(id);

            return Ok(new
            {
                message = "User deleted successfully."
            });
        }
    }
}
