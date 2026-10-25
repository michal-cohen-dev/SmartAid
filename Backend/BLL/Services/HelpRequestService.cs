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

        public bool Update(int id, HelpRequestDto helpRequestDto, int userId)
        {
            var existingRequest = _helpRequestRepository.GetOwned(id, userId);

            if (existingRequest == null)
                return false;

            _mapper.Map(helpRequestDto, existingRequest);

            // מוודאים שהבקשה נשארת בבעלות המשתמש המקורי
            existingRequest.UserId = userId;

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
