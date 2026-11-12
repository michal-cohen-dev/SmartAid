
using AutoMapper;
using BLL.Exceptions;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using Microsoft.EntityFrameworkCore;

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
        private readonly IHelpTypeRepository _helpTypeRepository;
        public HelpRequestService(
            IHelpRequestRepository helpRequestRepository,
            IMapper mapper,
            IImageService imageService,
            IRequestEquipmentRepository requestEquipmentRepository,
            IEquipmentTypeRepository equipmentTypeRepository,
            IHelpTypeRepository helpTypeRepository,
            IVolunteerService volunteerService)
        {
            _helpRequestRepository = helpRequestRepository;
            _mapper = mapper;
            _imageService = imageService;
            _requestEquipmentRepository = requestEquipmentRepository;
            _equipmentTypeRepository = equipmentTypeRepository;
            _helpTypeRepository = helpTypeRepository;
            _volunteerService = volunteerService;
        }

        public List<HelpRequestDto> GetAll()
        {
            var requests = _helpRequestRepository.GetAll();
            return _mapper.Map<List<HelpRequestDto>>(requests);
        }

        public List<HelpRequestDto> GetByUserId(int userId)
        {
            var requests = _helpRequestRepository.GetByUserId(userId);

            return _mapper.Map<List<HelpRequestDto>>(requests);
        }

        public List<HelpRequestPreviewDto> GetOpenForVolunteer(int userId)
        {
            var volunteer = _volunteerService.GetByUserId(userId);

            if (volunteer == null || !volunteer.IsActive)
                return new List<HelpRequestPreviewDto>();

            var requests = _helpRequestRepository.GetOpenRequests();

            var fullDtos = _mapper.Map<List<HelpRequestDto>>(requests);

            return fullDtos.Select(request => new HelpRequestPreviewDto
            {
                Id = request.Id,
                Title = request.Title,
                Description = request.Description,
                CreatedAt = request.CreatedAt,
                RequiredEquipmentTypeIds = request.RequiredEquipmentTypeIds,
                RequiredEquipmentNames = request.RequiredEquipmentNames
            }).ToList();
        }


        public HelpRequestDto? GetById(int id)
        {
            var request = _helpRequestRepository.GetById(id);

            if (request == null)
                return null;

            return _mapper.Map<HelpRequestDto>(request);
        }


        public bool IsOwner(int requestId, int userId)
        {
            var request = _helpRequestRepository.GetOwned(requestId, userId);

            return request != null;
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


        public int Add(HelpRequestPostDto helpRequestDto, int userId)
        {
            var equipmentTypeIds = helpRequestDto.RequiredEquipmentTypeIds
                .Distinct()
                .ToList();

            foreach (var equipmentTypeId in equipmentTypeIds)
            {
                if (_equipmentTypeRepository.GetById(equipmentTypeId) == null)
                {
                    throw new BusinessRuleException(
                        $"Equipment type with ID {equipmentTypeId} does not exist.");
                }
            }

            var helpType = _helpTypeRepository.GetById(
                helpRequestDto.HelpTypeId);

            if (helpType == null)
            {
                throw new BusinessRuleException(
                    $"Help type with ID {helpRequestDto.HelpTypeId} does not exist.");
            }

            var request = _mapper.Map<HelpRequest>(helpRequestDto);
            request.UserId = userId;
            request.HelpTypeId = helpRequestDto.HelpTypeId;

            string? savedImageName = null;

            try
            {
                if (helpRequestDto.ImageFile != null)
                {
                    savedImageName = _imageService.Save(helpRequestDto.ImageFile);
                    request.ImageName = savedImageName;
                }

                // שמירת הבקשה וקבלת המזהה שלה
                _helpRequestRepository.Add(request);

                // שמירת הציוד הנדרש בטבלת RequestEquipments
                foreach (var equipmentTypeId in equipmentTypeIds)
                {
                    _requestEquipmentRepository.Add(new RequestEquipment
                    {
                        HelpRequestId = request.Id,
                        EquipmentTypeId = equipmentTypeId
                    });
                }
            }
            catch
            {
                if (!string.IsNullOrEmpty(savedImageName))
                {
                    try
                    {
                        _imageService.Delete(savedImageName);
                    }
                    catch
                    {
                        // Preserve the original exception.
                    }
                }

                throw;
            }
            return request.Id;
        }

        

        public bool Update(
            int id,
            HelpRequestUpdateDto helpRequestDto,
            int userId)
        {
            var existingRequest = _helpRequestRepository.GetOwned(id, userId);

            if (existingRequest == null)
                throw new NotFoundException("Help request not found.");

            var equipmentTypeIds = helpRequestDto.RequiredEquipmentTypeIds
                .Distinct()
                .ToList();

            foreach (var equipmentTypeId in equipmentTypeIds)
            {
                if (_equipmentTypeRepository.GetById(equipmentTypeId) == null)
                {
                    throw new BusinessRuleException(
                        $"Equipment type with ID {equipmentTypeId} does not exist.");
                }
            }

            existingRequest.Title = helpRequestDto.Title;
            existingRequest.Description = helpRequestDto.Description;
            existingRequest.Latitude = helpRequestDto.Latitude;
            existingRequest.Longitude = helpRequestDto.Longitude;
            existingRequest.Address = helpRequestDto.Address;
            existingRequest.AddressNotes = helpRequestDto.AddressNotes;

            // בדיקה שסוג העזרה קיים במערכת
            var helpType = _helpTypeRepository.GetById(
                helpRequestDto.HelpTypeId);

            if (helpType == null)
            {
                throw new BusinessRuleException(
                    $"Help type with ID {helpRequestDto.HelpTypeId} does not exist.");
            }

            existingRequest.HelpTypeId = helpRequestDto.HelpTypeId;

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

        public bool TryAssignToVolunteer(int requestId, int userId)
        {
            // איתור פרופיל המתנדב לפי המשתמש המחובר
            var volunteer = _volunteerService.GetByUserId(userId);

            // רק מתנדב פעיל יכול לקבל בקשה
            if (volunteer == null || !volunteer.IsActive)
                return false;

            // הקצאה אטומית: רק מתנדב אחד יוכל לקבל את הבקשה
            return _helpRequestRepository.TryAssignToVolunteer(
                requestId,
                volunteer.Id);
        }

        public bool TryMarkInTreatment(int requestId, int userId)
        {
            // איתור פרופיל המתנדב לפי המשתמש המחובר
            var volunteer = _volunteerService.GetByUserId(userId);

            // רק מתנדב פעיל יכול לסמן שהגיע
            if (volunteer == null || !volunteer.IsActive)
                return false;

            // שינוי הסטטוס מ-Assigned ל-InTreatment
            return _helpRequestRepository.TryMarkInTreatment(
                requestId,
                volunteer.Id);
        }

        public bool TryCompleteRequest(int requestId, int userId)
        {
            var volunteer = _volunteerService.GetByUserId(userId);

            if (volunteer == null || !volunteer.IsActive)
                return false;

            return _helpRequestRepository.TryCompleteRequest(
                requestId,
                volunteer.Id);
        }
    }
}
