
using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services.Interfaces;

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
            // המשתמש יכול לצפות רק בפרופיל של עצמו
            if (id != currentUserId)
                return null;

            var user = _userRepository.GetById(id);

            if (user == null)
                return null;

            return _mapper.Map<UserDto>(user);
        }


        public bool Update(int id, UserUpdateDto userDto, int currentUserId)
        {
            // המשתמש יכול לעדכן רק את הפרופיל של עצמו
            if (id != currentUserId)
                return false;

            var existingUser = _userRepository.GetById(id);

            if (existingUser == null)
                return false;

            if (string.IsNullOrWhiteSpace(userDto.FullName) ||
                string.IsNullOrWhiteSpace(userDto.Email))
                return false;

            var email = userDto.Email.Trim();

            // בדיקה אם כתובת הדוא"ל כבר שייכת למשתמש אחר
            var userWithEmail = _userRepository.GetByEmail(email);

            if (userWithEmail != null && userWithEmail.Id != id)
                return false;

            existingUser.FullName = userDto.FullName.Trim();
            existingUser.Email = email;
            existingUser.PhoneNumber = userDto.PhoneNumber?.Trim();

            _userRepository.Update(existingUser);

            return true;
        }


        public bool Delete(int id, int currentUserId)
        {
            // משתמש לא יכול למחוק משתמש אחר
            if (id != currentUserId)
                return false;

            var existingUser = _userRepository.GetById(id);

            if (existingUser == null)
                return false;

            _userRepository.Delete(id);

            return true;
        }
    }
}
