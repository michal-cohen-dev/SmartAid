
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
        private readonly IEquipmentTypeRepository _equipmentTypeRepository;
        private readonly IMapper _mapper;
        private readonly IHelpTypeRepository _helpTypeRepository;

        public VolunteerService(
            IVolunteerRepository volunteerRepository,
            IUserRepository userRepository,
            IEquipmentTypeRepository equipmentTypeRepository,
            IHelpTypeRepository helpTypeRepository,
            IMapper mapper)
        {
            _volunteerRepository = volunteerRepository;
            _userRepository = userRepository;
            _equipmentTypeRepository = equipmentTypeRepository;
            _helpTypeRepository = helpTypeRepository;
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

        // רישום משתמש קיים כמתנדב
        public void Add(RegisterVolunteerDto volunteerDto, int currentUserId)
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

            var equipmentIds = volunteerDto.EquipmentTypeIds
                .Distinct()
                .ToList();

            if (equipmentIds.Count != volunteerDto.EquipmentTypeIds.Count)
            {
                throw new ConflictException(
                    "Duplicate equipment types are not allowed.");
            }

            if (equipmentIds.Any(id => id <= 0))
            {
                throw new ArgumentException(
                    "Invalid equipment type ID.");
            }

            foreach (var equipmentId in equipmentIds)
            {
                var equipment =
                    _equipmentTypeRepository.GetById(equipmentId);

                if (equipment == null)
                {
                    throw new NotFoundException(
                        $"Equipment type {equipmentId} was not found.");
                }
            }

            var helpTypeIds = volunteerDto.HelpTypeIds
                .Distinct()
                .ToList();

            if (helpTypeIds.Count == 0)
            {
                throw new ArgumentException(
                    "At least one help type must be selected.");
            }

            if (helpTypeIds.Count != volunteerDto.HelpTypeIds.Count)
            {
                throw new ConflictException(
                    "Duplicate help types are not allowed.");
            }

            if (helpTypeIds.Any(id => id <= 0))
            {
                throw new ArgumentException(
                    "Invalid help type ID.");
            }

            foreach (var helpTypeId in helpTypeIds)
            {
                var helpType = _helpTypeRepository.GetById(helpTypeId);

                if (helpType == null)
                {
                    throw new NotFoundException(
                        $"Help type {helpTypeId} was not found.");
                }
            }

            var volunteer = new Volunteer
            {
                UserId = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,

                Latitude = volunteerDto.Latitude,
                Longitude = volunteerDto.Longitude,

                IsActive = false,

                TotalOffersReceived = 0,
                TotalOffersAccepted = 0,
                TotalCompleted = 0,

                ReliabilityScore = 100.0,

                LastLocationUpdatedAt =
                    volunteerDto.Latitude.HasValue &&
                    volunteerDto.Longitude.HasValue
                        ? DateTime.UtcNow
                        : null
            };

            _volunteerRepository.AddWithEquipmentAndHelpTypes(
                volunteer,
                equipmentIds,
                helpTypeIds);
        }

        public void Update(
            int id,
            VolunteerDto volunteerDto,
            int currentUserId)
        {
            var existingVolunteer =
                _volunteerRepository.GetById(id);

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
            var existingVolunteer =
                _volunteerRepository.GetById(id);

            if (existingVolunteer == null ||
                existingVolunteer.UserId != currentUserId)
            {
                throw new NotFoundException(
                    "Volunteer profile not found.");
            }

            _volunteerRepository.Delete(id);
        }

        public void DecreaseReliabilityScore(int volunteerId)
        {
            _volunteerRepository.DecreaseReliabilityScore(volunteerId);
        }

        public void IncreaseReliabilityScore(int volunteerId)
        {
            _volunteerRepository.IncreaseReliabilityScore(volunteerId);
        }

        public void UpdateLocation(
            int userId,
            double latitude,
            double longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentException("Invalid latitude.");

            if (longitude < -180 || longitude > 180)
                throw new ArgumentException("Invalid longitude.");

            var volunteer = _volunteerRepository.GetByUserId(userId);

            if (volunteer == null)
                throw new NotFoundException("Volunteer profile not found.");

            volunteer.Latitude = latitude;
            volunteer.Longitude = longitude;
            volunteer.LastLocationUpdatedAt = DateTime.UtcNow;

            _volunteerRepository.Update(volunteer);
        }

        public void UpdateAvailability(int userId, bool isAvailable)
        {
            var volunteer = _volunteerRepository.GetByUserId(userId);

            if (volunteer == null)
                throw new NotFoundException("Volunteer profile not found.");

            if (isAvailable)
            {
                var locationFreshnessCutoff =
                    DateTime.UtcNow.AddMinutes(-5);

                if (!volunteer.Latitude.HasValue ||
                    !volunteer.Longitude.HasValue ||
                    !volunteer.LastLocationUpdatedAt.HasValue ||
                    volunteer.LastLocationUpdatedAt.Value <
                        locationFreshnessCutoff)
                {
                    throw new ConflictException(
                        "A fresh location is required to become available.");
                }
            }

            volunteer.IsActive = isAvailable;

            _volunteerRepository.Update(volunteer);
        }
    }
}
