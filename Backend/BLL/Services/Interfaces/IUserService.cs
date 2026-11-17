
using System.Collections.Generic;
using DTO;

namespace BLL.Services.Interfaces
{
    public interface IUserService
    {
        // פעולות משתמש רגיל - ניהול הפרופיל האישי
        UserDto? GetById(int id, int currentUserId);

        bool Update(
            int id,
            UserUpdateDto userDto,
            int currentUserId);

        bool Delete(int id, int currentUserId);

        // פעולות מנהל - ניהול כלל המשתמשים
        List<UserDto> GetAllForAdmin();

        UserDto? GetByIdForAdmin(int id);

        bool UpdateForAdmin(
            int id,
            UserUpdateDto userDto);

        bool DeleteForAdmin(int id);
    }
}
