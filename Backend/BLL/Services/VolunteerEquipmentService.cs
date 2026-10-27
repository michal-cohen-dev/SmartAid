
using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services.Interfaces;

namespace BLL.Services
{
    public class VolunteerEquipmentService : IVolunteerEquipmentService
    {
        private readonly IVolunteerEquipmentRepository _repository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IEquipmentTypeRepository _equipmentTypeRepository;
        private readonly IMapper _mapper;

        public VolunteerEquipmentService(
            IVolunteerEquipmentRepository repository,
            IVolunteerRepository volunteerRepository,
            IEquipmentTypeRepository equipmentTypeRepository,
            IMapper mapper)
        {
            _repository = repository;
            _volunteerRepository = volunteerRepository;
            _equipmentTypeRepository = equipmentTypeRepository;
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

        public List<VolunteerEquipmentDto> GetByVolunteerId(int volunteerId)
        {
            var items = _repository.GetByVolunteerId(volunteerId);

            return _mapper.Map<List<VolunteerEquipmentDto>>(items);
        }


        public void Add(VolunteerEquipmentDto dto)
        {
            if (dto.VolunteerId <= 0)
                throw new ArgumentException("Invalid volunteer ID.");

            if (dto.EquipmentTypeId <= 0)
                throw new ArgumentException("Invalid equipment type ID.");

            var volunteer = _volunteerRepository.GetById(dto.VolunteerId);

            if (volunteer == null)
                throw new ArgumentException("Volunteer not found.");

            var equipmentType = _equipmentTypeRepository.GetById(dto.EquipmentTypeId);

            if (equipmentType == null)
                throw new ArgumentException("Equipment type not found.");

            var item = new VolunteerEquipment
            {
                VolunteerId = volunteer.Id,
                EquipmentTypeId = equipmentType.Id
            };

            _repository.Add(item);
        }

        public void Update(int id, VolunteerEquipmentDto dto)
        {
            if (dto.EquipmentTypeId <= 0)
                throw new ArgumentException("Invalid equipment type ID.");

            var existing = _repository.GetById(id);

            if (existing == null)
                return;

            var equipmentType = _equipmentTypeRepository.GetById(dto.EquipmentTypeId);

            if (equipmentType == null)
                throw new ArgumentException("Equipment type not found.");

            existing.EquipmentTypeId = equipmentType.Id;

            _repository.Update(existing);
        }



        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
