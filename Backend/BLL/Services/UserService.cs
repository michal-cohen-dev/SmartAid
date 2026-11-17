
using AutoMapper;
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

        public UserService(
            IUserRepository userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        // פעולות משתמש רגיל

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

        public bool Update(
            int id,
            UserUpdateDto userDto,
            int currentUserId)
        {
            if (id != currentUserId)
                throw new BusinessRuleException(
                    "You can only update your own profile.");

            UpdateUserDetails(id, userDto);

            return true;
        }

        public bool Delete(int id, int currentUserId)
        {
            if (id != currentUserId)
                throw new BusinessRuleException(
                    "You can only delete your own account.");

            DeleteUser(id);

            return true;
        }

        // פעולות מנהל

        public List<UserDto> GetAllForAdmin()
        {
            var users = _userRepository.GetAll();

            return _mapper.Map<List<UserDto>>(users);
        }

        public UserDto? GetByIdForAdmin(int id)
        {
            var user = _userRepository.GetById(id);

            if (user == null)
                throw new NotFoundException("User not found.");

            return _mapper.Map<UserDto>(user);
        }

        public bool UpdateForAdmin(
            int id,
            UserUpdateDto userDto)
        {
            UpdateUserDetails(id, userDto);

            return true;
        }

        public bool DeleteForAdmin(int id)
        {
            DeleteUser(id);

            return true;
        }

        // פעולות משותפות

        private void UpdateUserDetails(
            int id,
            UserUpdateDto userDto)
        {
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

            var userWithEmail =
                _userRepository.GetByEmail(email);

            if (userWithEmail != null &&
                userWithEmail.Id != id)
            {
                throw new ConflictException(
                    "This email is already in use.");
            }

            existingUser.FullName = userDto.FullName.Trim();
            existingUser.Email = email;
            existingUser.PhoneNumber =
                userDto.PhoneNumber?.Trim() ?? string.Empty;

            _userRepository.Update(existingUser);
        }

        private void DeleteUser(int id)
        {
            var existingUser = _userRepository.GetById(id);

            if (existingUser == null)
                throw new NotFoundException("User not found.");

            if (_userRepository.HasRestrictingDependencies(id))
            {
                throw new ConflictException(
                    "Cannot delete this user because they have an associated volunteer profile or help requests.");
            }

            _userRepository.Delete(id);
        }
    }
}
