using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;

namespace BLL.Services.Interfaces
{
    public interface IUserService
    {
        List<UserDto> GetAll();
        UserDto? GetById(int id);
        void Add(UserDto userDto);
        void Update(int id, UserDto userDto);
        void Delete(int id);
    }
}
