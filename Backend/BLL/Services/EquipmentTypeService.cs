using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services;

namespace BLL.Services
{
    public class EquipmentTypeService : IEquipmentTypeService
    {
        private readonly IEquipmentTypeRepository _repository;
        private readonly IMapper _mapper;

        public EquipmentTypeService(
            IEquipmentTypeRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public List<EquipmentTypeDto> GetAll()
        {
            var equipmentTypes = _repository.GetAll();
            return _mapper.Map<List<EquipmentTypeDto>>(equipmentTypes);
        }

        public EquipmentTypeDto? GetById(int id)
        {
            var equipmentType = _repository.GetById(id);

            if (equipmentType == null)
                return null;

            return _mapper.Map<EquipmentTypeDto>(equipmentType);
        }

        public void Add(EquipmentTypeDto equipmentTypeDto)
        {
            var equipmentType = _mapper.Map<EquipmentType>(equipmentTypeDto);
            _repository.Add(equipmentType);
        }

        public void Update(int id, EquipmentTypeDto equipmentTypeDto)
        {
            var existingEquipmentType = _repository.GetById(id);

            if (existingEquipmentType == null)
                return;

            var equipmentType = _mapper.Map<EquipmentType>(equipmentTypeDto);
            equipmentType.Id = id;

            _repository.Update(equipmentType);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
