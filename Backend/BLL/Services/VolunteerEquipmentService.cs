
using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services;

namespace BLL.Services
{
    public class VolunteerEquipmentService : IVolunteerEquipmentService
    {
        private readonly IVolunteerEquipmentRepository _repository;
        private readonly IMapper _mapper;

        public VolunteerEquipmentService(
            IVolunteerEquipmentRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public List<VolunteerEquipmentDto> GetAll()
        {
            var items = _repository.GetAll();
            return _mapper.Map<List<VolunteerEquipmentDto>>(items);
        }

        public VolunteerEquipmentDto? GetById(int id)
        {
            var item = _repository.GetById(id);

            if (item == null)
                return null;

            return _mapper.Map<VolunteerEquipmentDto>(item);
        }

        public void Add(VolunteerEquipmentDto dto)
        {
            var item = _mapper.Map<VolunteerEquipment>(dto);
            _repository.Add(item);
        }

        public void Update(int id, VolunteerEquipmentDto dto)
        {
            var existing = _repository.GetById(id);

            if (existing == null)
                return;

            var item = _mapper.Map<VolunteerEquipment>(dto);
            item.Id = id;

            _repository.Update(item);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
