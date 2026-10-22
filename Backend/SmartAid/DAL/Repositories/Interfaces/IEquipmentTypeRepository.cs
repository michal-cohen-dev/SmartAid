using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Interfaces
{
    public interface IEquipmentTypeRepository
    {
        List<EquipmentType> GetAll();
        EquipmentType? GetById(int id);
        void Add(EquipmentType equipmentType);
        void Update(EquipmentType equipmentType);
        void Delete(int id);
    }
}