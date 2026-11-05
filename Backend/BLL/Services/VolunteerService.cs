
using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services.Interfaces;
using BLL.Exceptions;

namespace BLL.Services
{
    public class VolunteerService : IVolunteerService
    {
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public VolunteerService(
            IVolunteerRepository volunteerRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _volunteerRepository = volunteerRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public List<VolunteerDto> GetAll()
        {
            var volunteers = _volunteerRepository.GetAll();
            return _mapper.Map<List<VolunteerDto>>(volunteers);
        }

        public VolunteerDto? GetById(int id)
        {
            var volunteer = _volunteerRepository.GetById(id);

            if (volunteer == null)
                return null;

            return _mapper.Map<VolunteerDto>(volunteer);
        }

        public VolunteerDto? GetByUserId(int userId)
        {
            var volunteer = _volunteerRepository.GetByUserId(userId);

            if (volunteer == null)
                return null;

            return _mapper.Map<VolunteerDto>(volunteer);
        }

        public void Add(VolunteerDto volunteerDto, int currentUserId)
        {
            var existingVolunteer =
                _volunteerRepository.GetByUserId(currentUserId);

            if (existingVolunteer != null)
            {
                throw new ConflictException(
                    "This user already has a volunteer profile.");
            }

            var user = _userRepository.GetById(currentUserId);

            if (user == null)
            {
                throw new NotFoundException(
                    "The current user was not found.");
            }

            var volunteer = new Volunteer
            {
                UserId = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = true,
                TotalOffersReceived = 0,
                TotalOffersAccepted = 0,
                TotalCompleted = 0,
                ReliabilityScore = 100.0
            };

            _volunteerRepository.Add(volunteer);
        }

        public void Update(int id, VolunteerDto volunteerDto, int currentUserId)
        {
            var existingVolunteer = _volunteerRepository.GetById(id);

            if (existingVolunteer == null ||
                existingVolunteer.UserId != currentUserId)
            {
                throw new NotFoundException(
                    "Volunteer profile not found.");
            }

            existingVolunteer.FullName = volunteerDto.FullName;
            existingVolunteer.PhoneNumber = volunteerDto.PhoneNumber;

            _volunteerRepository.Update(existingVolunteer);
        }

        public void Delete(int id, int currentUserId)
        {
            var existingVolunteer = _volunteerRepository.GetById(id);

            if (existingVolunteer == null ||
                existingVolunteer.UserId != currentUserId)
            {
                throw new NotFoundException(
                    "Volunteer profile not found.");
            }

            _volunteerRepository.Delete(id);
        }
    }
}
