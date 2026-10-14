using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL.Repositories.Interfaces
{
    public interface IUserRepository
    {
        List<UserDto> GetAll();
        UserDto GetById(int id);
        void Add(UserDto userDto);
        void Update(UserDto userDto);
        void Delete(int id);
    }
}
