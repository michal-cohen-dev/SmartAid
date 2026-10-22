using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Interfaces
{
    public interface IVolunteerEquipmentRepository
    {
        List<VolunteerEquipment> GetAll();
        VolunteerEquipment? GetById(int id);
        void Add(VolunteerEquipment volunteerEquipment);
        void Update(VolunteerEquipment volunteerEquipment);
        void Delete(int id);
    }
}