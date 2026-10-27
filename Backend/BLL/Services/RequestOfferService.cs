
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
        private readonly IMapper _mapper;

        public RequestOfferService(
            IRequestOfferRepository requestOfferRepository,
            IMapper mapper)
        {
            _requestOfferRepository = requestOfferRepository;
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

            var offer = new RequestOffer
            {
                HelpRequestId = requestOfferDto.HelpRequestId,
                VolunteerId = requestOfferDto.VolunteerId
            };

            _requestOfferRepository.Add(offer);
        }

        public void Update(int id, RequestOfferDto requestOfferDto)
        {
            var existingOffer = _requestOfferRepository.GetById(id);

            if (existingOffer == null)
                return;

            if (!Enum.IsDefined(typeof(OfferStatus), requestOfferDto.Status))
                throw new ArgumentException("Invalid offer status.");

            existingOffer.Status = (OfferStatus)requestOfferDto.Status;

            _requestOfferRepository.Update(existingOffer);
        }

        public void Delete(int id)
        {
            _requestOfferRepository.Delete(id);
        }
    }
}
