
using BLL.Exceptions;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            IVolunteerRepository volunteerRepository,
            ITokenService tokenService,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _volunteerRepository = volunteerRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public string Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException(
                    "Full name, email and password are required.");
            }

            var email = dto.Email.Trim();

            if (_userRepository.GetByEmail(email) != null)
            {
                throw new ConflictException(
                    "An account with this email already exists.");
            }

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                PhoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                dto.Password);

            _userRepository.Add(user);

            // בהרשמה ראשונית המשתמש עדיין אינו מתנדב
            return _tokenService.GenerateToken(
                user,
                false,
                null);
        }

        public string Login(LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException(
                    "Email and password are required.");
            }

            var email = dto.Email.Trim();
            var user = _userRepository.GetByEmail(email);

            if (user == null)
            {
                throw new InvalidCredentialsException(
                    "Invalid email or password.");
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                throw new InvalidCredentialsException(
                    "Invalid email or password.");
            }

            // בדיקה האם למשתמש יש פרופיל מתנדב
            var volunteer =
                _volunteerRepository.GetByUserId(user.Id);

            bool isVolunteer = volunteer != null;

            int? volunteerId = volunteer?.Id;

            // יצירת Token בהתאם למצב המשתמש
            return _tokenService.GenerateToken(
                user,
                isVolunteer,
                volunteerId);
        }
    }
}
