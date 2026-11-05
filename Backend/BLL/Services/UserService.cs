using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services.Interfaces;
using BLL.Exceptions;

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

        public UserDto? GetById(int id, int currentUserId)
        {
            if (id != currentUserId)
                throw new BusinessRuleException(
                    "You can only view your own profile.");

            var user = _userRepository.GetById(id);

            if (user == null)
                throw new NotFoundException("User not found.");

            return _mapper.Map<UserDto>(user);
        }

        public bool Update(int id, UserUpdateDto userDto, int currentUserId)
        {
            if (id != currentUserId)
                throw new BusinessRuleException(
                    "You can only update your own profile.");

            var existingUser = _userRepository.GetById(id);

            if (existingUser == null)
                throw new NotFoundException("User not found.");

            if (string.IsNullOrWhiteSpace(userDto.FullName) ||
                string.IsNullOrWhiteSpace(userDto.Email))
            {
                throw new BusinessRuleException(
                    "Full name and email are required.");
            }

            var email = userDto.Email.Trim();

            var userWithEmail = _userRepository.GetByEmail(email);

            if (userWithEmail != null && userWithEmail.Id != id)
                throw new ConflictException(
                    "This email is already in use.");

            existingUser.FullName = userDto.FullName.Trim();
            existingUser.Email = email;
            existingUser.PhoneNumber = userDto.PhoneNumber?.Trim();

            _userRepository.Update(existingUser);

            return true;
        }

        public bool Delete(int id, int currentUserId)
        {
            if (id != currentUserId)
                throw new BusinessRuleException(
                    "You can only delete your own account.");

            var existingUser = _userRepository.GetById(id);

            if (existingUser == null)
                throw new NotFoundException("User not found.");

            _userRepository.Delete(id);

            return true;
        }
    }
}