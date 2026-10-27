using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL.Services.Interfaces
{
    public interface IUserService
    {
        UserDto? GetById(int id, int currentUserId);

        bool Update(int id, UserUpdateDto userDto, int currentUserId);

        bool Delete(int id, int currentUserId);
    }
}
