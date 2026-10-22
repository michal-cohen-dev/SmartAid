
using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services;

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
            var offer = _mapper.Map<RequestOffer>(requestOfferDto);
            _requestOfferRepository.Add(offer);
        }

        public void Update(int id, RequestOfferDto requestOfferDto)
        {
            var existingOffer = _requestOfferRepository.GetById(id);

            if (existingOffer == null)
                return;

            var offer = _mapper.Map<RequestOffer>(requestOfferDto);
            offer.Id = id;

            _requestOfferRepository.Update(offer);
        }

        public void Delete(int id)
        {
            _requestOfferRepository.Delete(id);
        }
    }
}
