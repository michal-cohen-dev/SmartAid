
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using BLL.Exceptions;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class HelpRequestService : IHelpRequestService
    {
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IHelpRequestRepository _helpRequestRepository;
        private readonly IVolunteerService _volunteerService;
        private readonly IVolunteerMatchingService _volunteerMatchingService;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IRequestEquipmentRepository _requestEquipmentRepository;
        private readonly IEquipmentTypeRepository _equipmentTypeRepository;
        private readonly IHelpTypeRepository _helpTypeRepository;
        private readonly IRequestOfferService _requestOfferService;
        private readonly ILogger<HelpRequestService> _logger;

        public HelpRequestService(
            IHelpRequestRepository helpRequestRepository,
            IMapper mapper,
            IImageService imageService,
            IRequestEquipmentRepository requestEquipmentRepository,
            IEquipmentTypeRepository equipmentTypeRepository,
            IHelpTypeRepository helpTypeRepository,
            IVolunteerService volunteerService,
            IVolunteerRepository volunteerRepository,
            IVolunteerMatchingService volunteerMatchingService,
            IRequestOfferService requestOfferService,
            ILogger<HelpRequestService> logger)
        {
            _helpRequestRepository = helpRequestRepository;
            _mapper = mapper;
            _imageService = imageService;
            _requestEquipmentRepository = requestEquipmentRepository;
            _equipmentTypeRepository = equipmentTypeRepository;
            _helpTypeRepository = helpTypeRepository;
            _volunteerService = volunteerService;
            _volunteerRepository = volunteerRepository;
            _volunteerMatchingService = volunteerMatchingService;
            _requestOfferService = requestOfferService;
            _logger = logger;
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

        public List<HelpRequestDto> GetByVolunteerId(int userId)
        {
            var volunteer = _volunteerService.GetByUserId(userId);

            if (volunteer == null)
                return new List<HelpRequestDto>();

            var requests = _helpRequestRepository.GetByVolunteerId(volunteer.Id);

            return _mapper.Map<List<HelpRequestDto>>(requests);
        }

        public async Task<List<HelpRequestPreviewDto>> GetOpenForVolunteerAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var volunteer = _volunteerService.GetByUserId(userId);

            if (volunteer == null || !volunteer.IsActive)
                return new List<HelpRequestPreviewDto>();

            if (_helpRequestRepository.HasActiveAssignment(volunteer.Id))
                return new List<HelpRequestPreviewDto>();

            var matches = await _volunteerMatchingService
                .GetEligibleRequestsForVolunteerAsync(
                    volunteer.Id,
                    cancellationToken);

            var pendingOffers =
                _requestOfferService.GetPendingByVolunteerId(volunteer.Id);

            var pendingRequestIds = pendingOffers
                .Select(offer => offer.HelpRequestId)
                .ToList();

            var eligibleRequests = matches
                .Where(match =>
                    pendingRequestIds.Contains(match.HelpRequest.Id))
                .Select(match => match.HelpRequest)
                .ToList();

            var fullDtos = _mapper.Map<List<HelpRequestDto>>(
                eligibleRequests);

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

        public async Task<int> Add(
            HelpRequestPostDto helpRequestDto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            // Add the equipment to the request graph.
            // EF Core will save the request and its equipment
            // together in one SaveChanges transaction.
            request.RequiredEquipments = equipmentTypeIds
                .Select(equipmentTypeId => new RequestEquipment
                {
                    EquipmentTypeId = equipmentTypeId
                })
                .ToList();

            string? savedImageName = null;

            try
            {
                if (helpRequestDto.ImageFile != null)
                {
                    savedImageName = _imageService.Save(
                        helpRequestDto.ImageFile);

                    request.ImageName = savedImageName;
                }

                // One repository call and one SaveChanges:
                // request + all required equipment are saved together.
                _helpRequestRepository.Add(request);
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

            // Failure to create offers must not undo the request.
            try
            {
                await _requestOfferService.CreateOffersForRequestAsync(
                    request.Id,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create offers for help request {RequestId}.",
                    request.Id);
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

            // A request can only be edited while it is open
            // and has not yet been assigned to a volunteer.
            if (existingRequest.Status != RequestStatus.Open ||
                existingRequest.AssignedVolunteerId != null)
            {
                throw new BusinessRuleException(
                    "A help request can only be edited while it is open and has not been assigned to a volunteer.");
            }

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

            existingRequest.Title = helpRequestDto.Title;
            existingRequest.Description = helpRequestDto.Description;
            existingRequest.Latitude = helpRequestDto.Latitude;
            existingRequest.Longitude = helpRequestDto.Longitude;
            existingRequest.Address = helpRequestDto.Address;
            existingRequest.AddressNotes = helpRequestDto.AddressNotes;
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
            // Cancel the request instead of physically deleting it.
            return _helpRequestRepository.TryCancelRequest(
                id,
                userId,
                cancelledByVolunteer: false);
        }

        public bool TryAssignToVolunteer(int requestId, int userId)
        {
            var volunteer = _volunteerService.GetByUserId(userId);

            if (volunteer == null || !volunteer.IsActive)
                return false;

            return _helpRequestRepository.TryAssignToVolunteer(
                requestId,
                volunteer.Id);
        }

        public bool TryMarkInTreatment(int requestId, int userId)
        {
            var volunteer = _volunteerService.GetByUserId(userId);

            if (volunteer == null || !volunteer.IsActive)
                return false;

            return _helpRequestRepository.TryMarkInTreatment(
                requestId,
                volunteer.Id);
        }

        public bool TryCompleteRequest(int requestId, int userId)
        {
            var volunteer = _volunteerService.GetByUserId(userId);

            if (volunteer == null || !volunteer.IsActive)
                return false;

            using var transaction = _volunteerRepository.BeginTransaction();

            bool completed = _helpRequestRepository.TryCompleteRequest(
                requestId,
                volunteer.Id);

            if (!completed)
                return false;

            _volunteerRepository.IncrementTotalCompleted(volunteer.Id);
            _volunteerService.IncreaseReliabilityScore(volunteer.Id);

            transaction.Commit();

            return true;
        }

        public bool TryRateCompletedRequest(
            int requestId,
            int userId,
            int rating)
        {
            if (rating < 1 || rating > 5)
                return false;

            return _helpRequestRepository.TryRateCompletedRequest(
                requestId,
                userId,
                rating);
        }

        public bool TryCancelRequest(int requestId, int userId)
        {
            var request = _helpRequestRepository.GetById(requestId);

            if (request == null)
                return false;

            if (request.UserId == userId)
            {
                return _helpRequestRepository.TryCancelRequest(
                    requestId,
                    userId,
                    cancelledByVolunteer: false);
            }

            var volunteer = _volunteerService.GetByUserId(userId);

            if (volunteer == null ||
                request.AssignedVolunteerId != volunteer.Id)
            {
                return false;
            }

            using var transaction = _volunteerRepository.BeginTransaction();

            bool cancelled = _helpRequestRepository.TryCancelRequest(
                requestId,
                userId,
                cancelledByVolunteer: true);

            if (!cancelled)
                return false;

            _volunteerService.DecreaseReliabilityScore(volunteer.Id);

            transaction.Commit();

            return true;
        }
    }
}
