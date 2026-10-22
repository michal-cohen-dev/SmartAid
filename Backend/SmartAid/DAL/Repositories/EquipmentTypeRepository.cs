using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class EquipmentTypeRepository : IEquipmentTypeRepository
    {
        private readonly AppDbContext _context;

        public EquipmentTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<EquipmentType> GetAll()
        {
            return _context.EquipmentTypes.ToList();
        }

        public EquipmentType? GetById(int id)
        {
            return _context.EquipmentTypes.FirstOrDefault(e => e.Id == id);
        }

        public void Add(EquipmentType equipmentType)
        {
            _context.EquipmentTypes.Add(equipmentType);
            _context.SaveChanges();
        }

        public void Update(EquipmentType equipmentType)
        {
            var entity = _context.EquipmentTypes.Find(equipmentType.Id);

            if (entity != null)
            {
                entity.Name = equipmentType.Name;

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