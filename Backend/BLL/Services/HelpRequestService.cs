using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services.Interfaces;

namespace BLL.Services
{
    public class HelpRequestService : IHelpRequestService
    {
        private readonly IHelpRequestRepository _helpRequestRepository;
        private readonly IVolunteerService _volunteerService;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IRequestEquipmentRepository _requestEquipmentRepository;
        private readonly IEquipmentTypeRepository _equipmentTypeRepository;

        public HelpRequestService(
     IHelpRequestRepository helpRequestRepository,
     IMapper mapper,
     IImageService imageService,
     IRequestEquipmentRepository requestEquipmentRepository,
     IEquipmentTypeRepository equipmentTypeRepository,
     IVolunteerService volunteerService)
        {
            _helpRequestRepository = helpRequestRepository;
            _mapper = mapper;
            _imageService = imageService;
            _requestEquipmentRepository = requestEquipmentRepository;
            _equipmentTypeRepository = equipmentTypeRepository;
            _volunteerService = volunteerService;
        }

        public List<HelpRequestDto> GetAll()
        {
            var requests = _helpRequestRepository.GetAll();
            return _mapper.Map<List<HelpRequestDto>>(requests);
        }

        public HelpRequestDto? GetById(int id)
        {
            var request = _helpRequestRepository.GetById(id);

            if (request == null)
                return null;

            return _mapper.Map<HelpRequestDto>(request);
        }

        public bool IsAssignedVolunteer(int requestId, int userId)
        {
            var request = _helpRequestRepository.GetById(requestId);

            if (request == null || request.AssignedVolunteerId == null)
                return false;

            var volunteer = _volunteerService.GetByUserId(userId);

            if (volunteer == null)
                return false;

            return request.AssignedVolunteerId == volunteer.Id;
        }

        public void Add(HelpRequestPostDto helpRequestDto, int userId)
        {
            var request = _mapper.Map<HelpRequest>(helpRequestDto);
            request.UserId = userId;
            if (helpRequestDto.ImageFile != null)
            {
                var imageName = _imageService.Save(helpRequestDto.ImageFile);
                request.ImageName = imageName;
            }

            _helpRequestRepository.Add(request);
        }


        public bool Update(
            int id,
            HelpRequestUpdateDto helpRequestDto,
            int userId)
        {
            // מוודאים שהבקשה קיימת ושייכת למשתמש המחובר
            var existingRequest = _helpRequestRepository.GetOwned(id, userId);

            if (existingRequest == null)
                return false;

            // בודקים את כל סוגי הציוד לפני ביצוע שינויים
            var equipmentTypeIds = helpRequestDto.RequiredEquipmentTypeIds
                .Distinct()
                .ToList();

            foreach (var equipmentTypeId in equipmentTypeIds)
            {
                if (_equipmentTypeRepository.GetById(equipmentTypeId) == null)
                    return false;
            }

            // מעדכנים רק שדות שהמשתמש רשאי לשנות
            existingRequest.Latitude = helpRequestDto.Latitude;
            existingRequest.Longitude = helpRequestDto.Longitude;
            existingRequest.Address = helpRequestDto.Address;
            existingRequest.AddressNotes = helpRequestDto.AddressNotes;

            // מחליפים את סוגי הציוד הנדרשים
            var existingEquipments =
                _requestEquipmentRepository.GetByHelpRequestId(id);

            foreach (var equipment in existingEquipments)
            {
                _requestEquipmentRepository.Delete(equipment.Id);
            }

            foreach (var equipmentTypeId in equipmentTypeIds)
            {
                _requestEquipmentRepository.Add(new RequestEquipment
                {
                    HelpRequestId = id,
                    EquipmentTypeId = equipmentTypeId
                });
            }

            _helpRequestRepository.Update(existingRequest);

            return true;
        }

        public bool Delete(int id, int userId)
        {
            var existingRequest = _helpRequestRepository.GetOwned(id, userId);

            if (existingRequest == null)
                return false;

            _helpRequestRepository.Delete(id);

            return true;
        }
    }
}
