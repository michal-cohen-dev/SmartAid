using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Interfaces
{
    public interface IRequestEquipmentRepository
    {
        List<RequestEquipment> GetAll();
        RequestEquipment? GetById(int id);
        void Add(RequestEquipment requestEquipment);
        void Update(RequestEquipment requestEquipment);
        void Delete(int id);
    }
}