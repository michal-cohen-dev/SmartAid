
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services.Interfaces;
using BLL.Exceptions;
using System.Threading;
using System.Threading.Tasks;

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

        public RequestOfferService(
            IRequestOfferRepository requestOfferRepository,
            IHelpRequestRepository helpRequestRepository,
            IVolunteerRepository volunteerRepository,
            IVolunteerMatchingService volunteerMatchingService,
            IMapper mapper)
        {
            _requestOfferRepository = requestOfferRepository;
            _helpRequestRepository = helpRequestRepository;
            _volunteerRepository = volunteerRepository;
            _volunteerMatchingService = volunteerMatchingService;
            _mapper = mapper;
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

            var offers = _requestOfferRepository.GetPendingByVolunteerId(volunteerId);

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




        public Task<bool> CreateOfferForVolunteerAsync(
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
                return Task.FromResult(false);

            var volunteer = _volunteerRepository.GetById(volunteerId);

            if (volunteer == null)
                throw new NotFoundException("Volunteer not found.");

            if (!volunteer.IsActive)
                return Task.FromResult(false);

            var offer = new RequestOffer
            {
                HelpRequestId = helpRequest.Id,
                VolunteerId = volunteer.Id
            };

            bool added = _requestOfferRepository.TryAddIfCapacityAvailable(
                offer,
                5);

            if (!added)
                return Task.FromResult(false);

            _volunteerRepository.IncrementTotalOffersReceived(volunteer.Id);

            return Task.FromResult(true);
        }



        public void Update(int id, RequestOfferDto requestOfferDto)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid request offer ID.");

            var existingOffer = _requestOfferRepository.GetById(id);

            if (existingOffer == null)
                throw new NotFoundException("Request offer not found.");

            if (!Enum.IsDefined(
                    typeof(OfferStatus), requestOfferDto.Status))
            {
                throw new ArgumentException("Invalid offer status.");
            }

            var previousStatus = existingOffer.Status;
            var newStatus = (OfferStatus)requestOfferDto.Status;

            existingOffer.Status = newStatus;

            _requestOfferRepository.Update(existingOffer);

            if (previousStatus == OfferStatus.Pending &&
                newStatus == OfferStatus.Accepted)
            {
                _volunteerRepository.IncrementTotalOffersAccepted(
                    existingOffer.VolunteerId);
            }
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

            await CreateOffersForRequestAsync(
                offer.HelpRequestId,
                cancellationToken);

            return true;
        }

        public async Task<int> CreateOffersForRequestAsync(
            int helpRequestId,
            CancellationToken cancellationToken = default)
        {
            if (helpRequestId <= 0)
                throw new ArgumentException("Invalid help request ID.");

            cancellationToken.ThrowIfCancellationRequested();

            var helpRequest = _helpRequestRepository.GetById(helpRequestId);

            if (helpRequest == null)
                throw new NotFoundException("Help request not found.");

            if (helpRequest.Status != RequestStatus.Open)
                return 0;

            var matches =
                await _volunteerMatchingService.GetEligibleVolunteersAsync(
                    helpRequestId,
                    cancellationToken);

            int createdOffers = 0;

            foreach (var match in matches)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var offer = new RequestOffer
                {
                    HelpRequestId = helpRequestId,
                    VolunteerId = match.Volunteer.Id
                };

                bool added =
                    _requestOfferRepository.TryAddIfCapacityAvailable(
                        offer,
                        MaxActiveOffers);

                if (added)
                {
                    createdOffers++;

                    _volunteerRepository.IncrementTotalOffersReceived(
                        match.Volunteer.Id);
                }
            }

            return createdOffers;
        }

        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid request offer ID.");

            var existingOffer = _requestOfferRepository.GetById(id);

            if (existingOffer == null)
                throw new NotFoundException("Request offer not found.");

            _requestOfferRepository.Delete(id);
        }
    }
}
