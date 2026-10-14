using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BLL.Repositories.Interfaces;
using DAL;
using DAL.Entities;
using DTO;

namespace BLL.Repositories
{
    public class EquipmentTypeRepository : IEquipmentTypeRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public EquipmentTypeRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<EquipmentTypeDto> GetAll()
        {
            return _context.EquipmentTypes
                .ProjectTo<EquipmentTypeDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public EquipmentTypeDto GetById(int id)
        {
            return _context.EquipmentTypes
                .Where(e => e.Id == id)
                .ProjectTo<EquipmentTypeDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault()!;
        }

        public void Add(EquipmentTypeDto equipmentTypeDto)
        {
            var entity = _mapper.Map<EquipmentType>(equipmentTypeDto);
            _context.EquipmentTypes.Add(entity);
            _context.SaveChanges(); // סינכרוני בלבד
        }

        public void Update(EquipmentTypeDto equipmentTypeDto)
        {
            var entity = _context.EquipmentTypes.Find(equipmentTypeDto.Id);
            if (entity != null)
            {
                _mapper.Map(equipmentTypeDto, entity);
                _context.EquipmentTypes.Update(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.EquipmentTypes.Find(id);
            if (entity != null)
            {
                _context.EquipmentTypes.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
