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
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public HelpRequestService(
     IHelpRequestRepository helpRequestRepository,
     IMapper mapper,
     IImageService imageService)
        {
            _helpRequestRepository = helpRequestRepository;
            _mapper = mapper;
            _imageService = imageService;
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

        public void Add(HelpRequestPostDto helpRequestDto)
        {
            var request = _mapper.Map<HelpRequest>(helpRequestDto);

            if (helpRequestDto.ImageFile != null)
            {
                var imageName = _imageService.Save(helpRequestDto.ImageFile);
                request.ImageName = imageName;
            }

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
