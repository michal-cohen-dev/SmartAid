
using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services.Interfaces;

namespace BLL.Services
{
    public class RequestEquipmentService : IRequestEquipmentService
    {
        private readonly IRequestEquipmentRepository _repository;
        private readonly IMapper _mapper;

        public RequestEquipmentService(
            IRequestEquipmentRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public List<RequestEquipmentDto> GetAll()
        {
            var items = _repository.GetAll();
            return _mapper.Map<List<RequestEquipmentDto>>(items);
        }

        public RequestEquipmentDto? GetById(int id)
        {
            var item = _repository.GetById(id);

            if (item == null)
                return null;

            return _mapper.Map<RequestEquipmentDto>(item);
        }

        public void Add(RequestEquipmentDto dto)
        {
            var item = _mapper.Map<RequestEquipment>(dto);
            _repository.Add(item);
        }

        public void Update(int id, RequestEquipmentDto dto)
        {
            var existing = _repository.GetById(id);

            if (existing == null)
                return;

            var item = _mapper.Map<RequestEquipment>(dto);
            item.Id = id;

            _repository.Update(item);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
