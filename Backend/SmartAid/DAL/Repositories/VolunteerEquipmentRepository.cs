using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class VolunteerEquipmentRepository : IVolunteerEquipmentRepository
    {
        private readonly AppDbContext _context;

        public VolunteerEquipmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<VolunteerEquipment> GetAll()
        {
            return _context.VolunteerEquipments
                .Include(ve => ve.Volunteer)
                .Include(ve => ve.EquipmentType)
                .ToList();
        }

        public VolunteerEquipment? GetById(int id)
        {
            return _context.VolunteerEquipments
                .Include(ve => ve.Volunteer)
                .Include(ve => ve.EquipmentType)
                .FirstOrDefault(ve => ve.Id == id);
        }
        public List<VolunteerEquipment> GetByVolunteerId(int volunteerId)
        {
            return _context.VolunteerEquipments
                .Include(ve => ve.EquipmentType)
                .Where(ve => ve.VolunteerId == volunteerId)
                .ToList();
        }

        public void Add(VolunteerEquipment volunteerEquipment)
        {
            _context.VolunteerEquipments.Add(volunteerEquipment);
            _context.SaveChanges();
        }

        public void Update(VolunteerEquipment volunteerEquipment)
        {
            var entity = _context.VolunteerEquipments.Find(volunteerEquipment.Id);

            if (entity != null)
            {
                entity.VolunteerId = volunteerEquipment.VolunteerId;
                entity.EquipmentTypeId = volunteerEquipment.EquipmentTypeId;

                _context.VolunteerEquipments.Update(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.VolunteerEquipments.Find(id);

            if (entity != null)
            {
                _context.VolunteerEquipments.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}