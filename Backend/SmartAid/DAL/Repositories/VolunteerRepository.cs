
using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class VolunteerRepository : IVolunteerRepository
    {
        private readonly AppDbContext _context;

        public VolunteerRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Volunteer> GetAll()
        {
            return _context.Volunteers.ToList();
        }

        public Volunteer? GetById(int id)
        {
            return _context.Volunteers.FirstOrDefault(v => v.Id == id);
        }

        public void Add(Volunteer volunteer)
        {
            _context.Volunteers.Add(volunteer);
            _context.SaveChanges();
        }

        public void Update(Volunteer volunteer)
        {
            var entity = _context.Volunteers.Find(volunteer.Id);

            if (entity != null)
            {
                entity.FullName = volunteer.FullName;
                entity.PhoneNumber = volunteer.PhoneNumber;
                entity.IsActive = volunteer.IsActive;
                entity.TotalOffersReceived = volunteer.TotalOffersReceived;
                entity.TotalOffersAccepted = volunteer.TotalOffersAccepted;
                entity.TotalCompleted = volunteer.TotalCompleted;
                entity.ReliabilityScore = volunteer.ReliabilityScore;

                _context.Volunteers.Update(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.Volunteers.Find(id);

            if (entity != null)
            {
                _context.Volunteers.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
