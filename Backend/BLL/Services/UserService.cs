using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public List<UserDto> GetAll()
        {
            var users = _userRepository.GetAll();
            return _mapper.Map<List<UserDto>>(users);
        }

        public UserDto? GetById(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
                return null;

            return _mapper.Map<UserDto>(user);
        }

        public void Add(UserDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            user.CreatedAt = DateTime.UtcNow;
            _userRepository.Add(user);
        }

        public void Update(int id, UserDto userDto)
        {
            var existingUser = _userRepository.GetById(id);
            if (existingUser == null)
                return;

            var user = _mapper.Map<User>(userDto);
            user.Id = id;

            _userRepository.Update(user);
        }

        public void Delete(int id)
        {
            _userRepository.Delete(id);
        }
    }
}