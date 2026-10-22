using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services;

namespace BLL.Services
{
    public class HelpRequestService : IHelpRequestService
    {
        private readonly IHelpRequestRepository _helpRequestRepository;
        private readonly IMapper _mapper;

        public HelpRequestService(
            IHelpRequestRepository helpRequestRepository,
            IMapper mapper)
        {
            _helpRequestRepository = helpRequestRepository;
            _mapper = mapper;
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

        public void Add(HelpRequestDto helpRequestDto)
        {
            var request = _mapper.Map<HelpRequest>(helpRequestDto);
            _helpRequestRepository.Add(request);
        }

        public void Update(int id, HelpRequestDto helpRequestDto)
        {
            var existingRequest = _helpRequestRepository.GetById(id);

            if (existingRequest == null)
                return;

            var request = _mapper.Map<HelpRequest>(helpRequestDto);
            request.Id = id;

            _helpRequestRepository.Update(request);
        }

        public void Delete(int id)
        {
            _helpRequestRepository.Delete(id);
        }
    }
}
