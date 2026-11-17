
using System;
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
    public class RequestOfferService : IRequestOfferService
    {
        private const int MaxActiveOffers = 5;

        private readonly IRequestOfferRepository _requestOfferRepository;
        private readonly IHelpRequestRepository _helpRequestRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IVolunteerMatchingService _volunteerMatchingService;
        private readonly IMapper _mapper;
        private readonly IVolunteerNotificationService _volunteerNotificationService;
        private readonly ILogger<RequestOfferService> _logger;

        public RequestOfferService(
            IRequestOfferRepository requestOfferRepository,
            IHelpRequestRepository helpRequestRepository,
            IVolunteerRepository volunteerRepository,
            IVolunteerMatchingService volunteerMatchingService,
            IMapper mapper,
            IVolunteerNotificationService volunteerNotificationService,
            ILogger<RequestOfferService> logger)
        {
            _requestOfferRepository = requestOfferRepository;
            _helpRequestRepository = helpRequestRepository;
            _volunteerRepository = volunteerRepository;
            _volunteerMatchingService = volunteerMatchingService;
            _mapper = mapper;
            _volunteerNotificationService = volunteerNotificationService;
            _logger = logger;
        }

        public List<RequestOfferDto> GetAll()
        {
            var offers = _requestOfferRepository.GetAll();
            return _mapper.Map<List<RequestOfferDto>>(offers);
        }

        public RequestOfferDto? GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid request offer ID.");

            var offer = _requestOfferRepository.GetById(id);

            if (offer == null)
                return null;

            return _mapper.Map<RequestOfferDto>(offer);
        }

        public List<RequestOfferDto> GetPendingByVolunteerId(int volunteerId)
        {
            if (volunteerId <= 0)
                throw new ArgumentException("Invalid volunteer ID.");

            var offers =
                _requestOfferRepository.GetPendingByVolunteerId(volunteerId);

            return _mapper.Map<List<RequestOfferDto>>(offers);
        }

        public void Add(RequestOfferDto requestOfferDto)
        {
            if (requestOfferDto.HelpRequestId <= 0)
                throw new ArgumentException("Invalid help request ID.");

            if (requestOfferDto.VolunteerId <= 0)
                throw new ArgumentException("Invalid volunteer ID.");

            var helpRequest =
                _helpRequestRepository.GetById(requestOfferDto.HelpRequestId);

            if (helpRequest == null)
                throw new NotFoundException("Help request not found.");

            if (helpRequest.Status != RequestStatus.Open)
                throw new BusinessRuleException(
                    "Offers can only be created for open help requests.");

            var volunteer =
                _volunteerRepository.GetById(requestOfferDto.VolunteerId);

            if (volunteer == null)
                throw new NotFoundException("Volunteer not found.");

            if (!volunteer.IsActive)
                throw new BusinessRuleException(
                    "Offers can only be created for active volunteers.");

            var offer = new RequestOffer
            {
                HelpRequestId = helpRequest.Id,
                VolunteerId = volunteer.Id
            };

            bool added = _requestOfferRepository.TryAddIfCapacityAvailable(
                offer,
                MaxActiveOffers);

            if (added)
            {
                _volunteerRepository.IncrementTotalOffersReceived(
                    volunteer.Id);
            }
        }

        public async Task<bool> CreateOfferForVolunteerAsync(
            int helpRequestId,
            int volunteerId,
            CancellationToken cancellationToken = default)
        {
            if (helpRequestId <= 0)
                throw new ArgumentException("Invalid help request ID.");

            if (volunteerId <= 0)
                throw new ArgumentException("Invalid volunteer ID.");

            var helpRequest = _helpRequestRepository.GetById(helpRequestId);

            if (helpRequest == null)
                throw new NotFoundException("Help request not found.");

            if (helpRequest.Status != RequestStatus.Open ||
                helpRequest.AssignedVolunteerId != null)
                return false;

            var volunteer = _volunteerRepository.GetById(volunteerId);

            if (volunteer == null)
                throw new NotFoundException("Volunteer not found.");

            if (!volunteer.IsActive)
                return false;

            var offer = new RequestOffer
            {
                HelpRequestId = helpRequest.Id,
                VolunteerId = volunteer.Id
            };

            bool added = _requestOfferRepository.TryAddIfCapacityAvailable(
                offer,
                MaxActiveOffers);

            if (!added)
                return false;

            _volunteerRepository.IncrementTotalOffersReceived(volunteer.Id);

            await NotifyNewOfferSafelyAsync(
                volunteer.Id,
                helpRequest.Id,
                cancellationToken);

            return true;
        }

        public void Update(int id, RequestOfferDto requestOfferDto)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid request offer ID.");

            throw new BusinessRuleException(
                "Request offer status cannot be changed directly. " +
                "Use the dedicated accept, decline or expiration operations.");
        }

        public async Task<bool> TryDeclineOfferAsync(
            int offerId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (offerId <= 0)
                throw new ArgumentException("Invalid request offer ID.");

            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");

            var volunteer = _volunteerRepository.GetByUserId(userId);

            if (volunteer == null)
                throw new NotFoundException("Volunteer profile not found.");

            var offer = _requestOfferRepository.GetById(offerId);

            if (offer == null)
                throw new NotFoundException("Request offer not found.");

            bool declined = _requestOfferRepository.TryDeclineOffer(
                offerId,
                volunteer.Id);

            if (!declined)
                return false;

            cancellationToken.ThrowIfCancellationRequested();

            // לאחר דחייה, ניתן להציע את הקריאה למתנדבים מתאימים אחרים.
            await CreateOffersForRequestAsync(
                offer.HelpRequestId,
                cancellationToken);

            return true;
        }

        public async Task<int> CreateOffersForRequestAsync(
            int helpRequestId,
            CancellationToken cancellationToken = default,
            bool onlyNeverOffered = false)
        {
            if (helpRequestId <= 0)
                throw new ArgumentException("Invalid help request ID.");

            cancellationToken.ThrowIfCancellationRequested();

            var helpRequest = _helpRequestRepository.GetById(helpRequestId);

            if (helpRequest == null)
                throw new NotFoundException("Help request not found.");

            if (helpRequest.Status != RequestStatus.Open ||
                helpRequest.AssignedVolunteerId != null)
                return 0;

            var matches =
                await _volunteerMatchingService.GetEligibleVolunteersAsync(
                    helpRequestId,
                    cancellationToken);

            // במחזור הרגיל של שתי דקות נשלח הצעות רק למתנדבים
            // שלא קיבלו הצעה לקריאה הזאת בעבר.
            HashSet<int> previouslyOfferedVolunteerIds =
                onlyNeverOffered
                    ? _requestOfferRepository
                        .GetOfferedVolunteerIdsForRequest(helpRequestId)
                        .ToHashSet()
                    : new HashSet<int>();

            int createdOffers = 0;

            foreach (var match in matches)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int volunteerId = match.Volunteer.Id;

                if (onlyNeverOffered &&
                    previouslyOfferedVolunteerIds.Contains(volunteerId))
                {
                    continue;
                }

                var offer = new RequestOffer
                {
                    HelpRequestId = helpRequestId,
                    VolunteerId = volunteerId
                };

                bool added =
                    _requestOfferRepository.TryAddIfCapacityAvailable(
                        offer,
                        MaxActiveOffers);

                if (!added)
                    continue;

                createdOffers++;

                _volunteerRepository.IncrementTotalOffersReceived(
                    volunteerId);

                await NotifyNewOfferSafelyAsync(
                    volunteerId,
                    helpRequestId,
                    cancellationToken);
            }

            return createdOffers;
        }

        public async Task ReassignPendingOffersAfterVolunteerAssignmentAsync(
            int volunteerId,
            int acceptedRequestId,
            CancellationToken cancellationToken = default)
        {
            if (volunteerId <= 0)
                throw new ArgumentException("Invalid volunteer ID.");

            if (acceptedRequestId <= 0)
                throw new ArgumentException("Invalid help request ID.");

            cancellationToken.ThrowIfCancellationRequested();

            // ביטול כל ההצעות הממתינות של המתנדב,
            // למעט ההצעה של הקריאה שאותה קיבל.
            var affectedRequestIds =
                _requestOfferRepository.ExpirePendingOffersForVolunteerExceptRequest(
                    volunteerId,
                    acceptedRequestId);

            foreach (var requestId in affectedRequestIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await CreateOffersForRequestAsync(
                        requestId,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to redistribute offers for request {RequestId} " +
                        "after volunteer {VolunteerId} was assigned to request {AcceptedRequestId}.",
                        requestId,
                        volunteerId,
                        acceptedRequestId);
                }
            }
        }

        private async Task NotifyNewOfferSafelyAsync(
            int volunteerId,
            int helpRequestId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _volunteerNotificationService.NotifyNewOfferAsync(
                    volunteerId,
                    helpRequestId,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send new offer notification. " +
                    "VolunteerId: {VolunteerId}, HelpRequestId: {HelpRequestId}",
                    volunteerId,
                    helpRequestId);
            }
        }
    }
}
