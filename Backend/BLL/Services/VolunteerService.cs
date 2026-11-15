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
        public void Add(
            RegisterVolunteerDto volunteerDto,
            int currentUserId)
        {
            // בדיקה שהמשתמש עדיין אינו מתנדב
            var existingVolunteer =
                _volunteerRepository.GetByUserId(currentUserId);

            if (existingVolunteer != null)
            {
                throw new ConflictException(
                    "This user already has a volunteer profile.");
            }

            // שליפת המשתמש הקיים
            var user = _userRepository.GetById(currentUserId);

            if (user == null)
            {
                throw new NotFoundException(
                    "The current user was not found.");
            }

            // בדיקת תקינות רשימת הציוד
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

            // בדיקה שכל פריטי הציוד קיימים במערכת
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

            // בדיקת תקינות רשימת סוגי העזרה
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

            // בדיקה שכל סוגי העזרה קיימים במערכת
            foreach (var helpTypeId in helpTypeIds)
            {
                var helpType = _helpTypeRepository.GetById(helpTypeId);

                if (helpType == null)
                {
                    throw new NotFoundException(
                        $"Help type {helpTypeId} was not found.");
                }
            }

            // יצירת פרופיל המתנדב
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

            // שמירת המתנדב והציוד שלו
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
    }
}