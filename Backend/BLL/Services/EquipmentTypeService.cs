using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services.Interfaces;
using BLL.Exceptions;

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
            var normalizedName = equipmentTypeDto.Name.Trim();

            var nameAlreadyExists = _repository.GetAll()
                .Any(existing => string.Equals(
                    existing.Name,
                    normalizedName,
                    StringComparison.OrdinalIgnoreCase));

            if (nameAlreadyExists)
                throw new ConflictException(
                    "An equipment type with this name already exists.");

            var equipmentType = _mapper.Map<EquipmentType>(equipmentTypeDto);
            equipmentType.Name = normalizedName;

            _repository.Add(equipmentType);
        }

        public void Update(int id, EquipmentTypeDto equipmentTypeDto)
        {
            var existingEquipmentType = _repository.GetById(id);

            if (existingEquipmentType == null)
                throw new NotFoundException("Equipment type not found.");

            var normalizedName = equipmentTypeDto.Name.Trim();

            var nameAlreadyExists = _repository.GetAll()
                .Any(existing => existing.Id != id &&
                    string.Equals(
                        existing.Name,
                        normalizedName,
                        StringComparison.OrdinalIgnoreCase));

            if (nameAlreadyExists)
                throw new ConflictException(
                    "An equipment type with this name already exists.");

            var equipmentType = _mapper.Map<EquipmentType>(equipmentTypeDto);
            equipmentType.Id = id;
            equipmentType.Name = normalizedName;

            _repository.Update(equipmentType);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
