
using AutoMapper;
using BLL.Exceptions;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public class HelpTypeService : IHelpTypeService
    {
        private readonly IHelpTypeRepository _repository;
        private readonly IMapper _mapper;

        public HelpTypeService(
            IHelpTypeRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public List<HelpTypeDto> GetAll()
        {
            var helpTypes = _repository.GetAll();

            return _mapper.Map<List<HelpTypeDto>>(helpTypes);
        }

        public HelpTypeDto? GetById(int id)
        {
            var helpType = _repository.GetById(id);

            if (helpType == null)
                return null;

            return _mapper.Map<HelpTypeDto>(helpType);
        }

        public void Add(HelpTypeDto helpTypeDto)
        {
            if (helpTypeDto == null)
                throw new ArgumentException("Invalid help type data.");

            var normalizedName = helpTypeDto.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ArgumentException("Help type name is required.");

            var nameAlreadyExists = _repository.GetAll()
                .Any(existing => string.Equals(
                    existing.Name,
                    normalizedName,
                    StringComparison.OrdinalIgnoreCase));

            if (nameAlreadyExists)
                throw new ConflictException(
                    "A help type with this name already exists.");

            var helpType = _mapper.Map<HelpType>(helpTypeDto);
            helpType.Name = normalizedName;

            _repository.Add(helpType);
        }

        public void Update(int id, HelpTypeDto helpTypeDto)
        {
            if (helpTypeDto == null)
                throw new ArgumentException("Invalid help type data.");

            var existingHelpType = _repository.GetById(id);

            if (existingHelpType == null)
                throw new NotFoundException("Help type not found.");

            var normalizedName = helpTypeDto.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ArgumentException("Help type name is required.");

            var nameAlreadyExists = _repository.GetAll()
                .Any(existing => existing.Id != id &&
                    string.Equals(
                        existing.Name,
                        normalizedName,
                        StringComparison.OrdinalIgnoreCase));

            if (nameAlreadyExists)
                throw new ConflictException(
                    "A help type with this name already exists.");

            existingHelpType.Name = normalizedName;

            _repository.Update(existingHelpType);
        }


        public void Delete(int id)
        {
            var existingHelpType = _repository.GetById(id);

            if (existingHelpType == null)
                throw new NotFoundException("Help type not found.");

            if (_repository.IsInUse(id))
                throw new ConflictException(
                    "Cannot delete a help type that is assigned to requests or volunteers.");

            _repository.Delete(id);
        }

    }
}
