
using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services.Interfaces;

namespace BLL.Services
{
    public class RequestOfferService : IRequestOfferService
    {
        private readonly IRequestOfferRepository _requestOfferRepository;
        private readonly IHelpRequestRepository _helpRequestRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IMapper _mapper;

        public RequestOfferService(
            IRequestOfferRepository requestOfferRepository,
            IHelpRequestRepository helpRequestRepository,
            IVolunteerRepository volunteerRepository,
            IMapper mapper)
        {
            _requestOfferRepository = requestOfferRepository;
            _helpRequestRepository = helpRequestRepository;
            _volunteerRepository = volunteerRepository;
            _mapper = mapper;
        }

        public List<RequestOfferDto> GetAll()
        {
            var offers = _requestOfferRepository.GetAll();
            return _mapper.Map<List<RequestOfferDto>>(offers);
        }

        public RequestOfferDto? GetById(int id)
        {
            var offer = _requestOfferRepository.GetById(id);

            if (offer == null)
                return null;

            return _mapper.Map<RequestOfferDto>(offer);
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
                throw new ArgumentException("Help request not found.");

            if (helpRequest.Status != RequestStatus.Open)
                throw new InvalidOperationException(
                    "Offers can only be created for open help requests.");

            var volunteer =
                _volunteerRepository.GetById(requestOfferDto.VolunteerId);

            if (volunteer == null)
                throw new ArgumentException("Volunteer not found.");

            if (!volunteer.IsActive)
                throw new InvalidOperationException(
                    "Offers can only be created for active volunteers.");

            var offer = new RequestOffer
            {
                HelpRequestId = helpRequest.Id,
                VolunteerId = volunteer.Id
            };

            _requestOfferRepository.Add(offer);
        }

        public void Update(int id, RequestOfferDto requestOfferDto)
        {
            var existingOffer = _requestOfferRepository.GetById(id);

            if (existingOffer == null)
                return;

            if (!Enum.IsDefined(
                    typeof(OfferStatus), requestOfferDto.Status))
            {
                throw new ArgumentException("Invalid offer status.");
            }

            existingOffer.Status = (OfferStatus)requestOfferDto.Status;

            _requestOfferRepository.Update(existingOffer);
        }

        public void Delete(int id)
        {
            _requestOfferRepository.Delete(id);
        }
    }
}
