
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Linq;

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

        public Volunteer? GetByUserId(int userId)
        {
            return _context.Volunteers
                .FirstOrDefault(v => v.UserId == userId);
        }

        public Volunteer? GetByIdWithQualifications(int id)
        {
            return _context.Volunteers
                .Include(v => v.VolunteerHelpTypes)
                    .ThenInclude(vht => vht.HelpType)
                .Include(v => v.VolunteerEquipments)
                    .ThenInclude(ve => ve.EquipmentType)
                .FirstOrDefault(v => v.Id == id);
        }

        public Volunteer? GetByUserIdWithQualifications(int userId)
        {
            return _context.Volunteers
                .Include(v => v.VolunteerHelpTypes)
                    .ThenInclude(vht => vht.HelpType)
                .Include(v => v.VolunteerEquipments)
                    .ThenInclude(ve => ve.EquipmentType)
                .FirstOrDefault(v => v.UserId == userId);
        }

        public void Add(Volunteer volunteer)
        {
            _context.Volunteers.Add(volunteer);
            _context.SaveChanges();
        }

        public void Update(Volunteer volunteer)
        {
            var entity = _context.Volunteers.Find(volunteer.Id);

            if (entity == null)
                return;

            entity.FullName = volunteer.FullName;
            entity.PhoneNumber = volunteer.PhoneNumber;
            entity.Latitude = volunteer.Latitude;
            entity.Longitude = volunteer.Longitude;
            entity.LastLocationUpdatedAt = volunteer.LastLocationUpdatedAt;
            entity.IsActive = volunteer.IsActive;
            entity.TotalOffersReceived = volunteer.TotalOffersReceived;
            entity.TotalOffersAccepted = volunteer.TotalOffersAccepted;
            entity.TotalCompleted = volunteer.TotalCompleted;
            entity.ReliabilityScore = volunteer.ReliabilityScore;

            _context.SaveChanges();
        }

        public void ReplaceQualifications(
            int volunteerId,
            List<int> equipmentTypeIds,
            List<int> helpTypeIds)
        {
            var volunteer = _context.Volunteers
                .Include(v => v.VolunteerEquipments)
                .Include(v => v.VolunteerHelpTypes)
                .FirstOrDefault(v => v.Id == volunteerId);

            if (volunteer == null)
                return;

            _context.VolunteerEquipments.RemoveRange(
                volunteer.VolunteerEquipments);

            _context.VolunteerHelpTypes.RemoveRange(
                volunteer.VolunteerHelpTypes);

            foreach (var equipmentTypeId in equipmentTypeIds.Distinct())
            {
                volunteer.VolunteerEquipments.Add(
                    new VolunteerEquipment
                    {
                        VolunteerId = volunteerId,
                        EquipmentTypeId = equipmentTypeId
                    });
            }

            foreach (var helpTypeId in helpTypeIds.Distinct())
            {
                volunteer.VolunteerHelpTypes.Add(
                    new VolunteerHelpType
                    {
                        VolunteerId = volunteerId,
                        HelpTypeId = helpTypeId
                    });
            }

            _context.SaveChanges();
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

        public void IncrementTotalCompleted(int volunteerId)
        {
            _context.Volunteers
                .Where(v => v.Id == volunteerId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(v => v.TotalCompleted,
                        v => v.TotalCompleted + 1));
        }

        public void IncrementTotalOffersReceived(int volunteerId)
        {
            _context.Volunteers
                .Where(v => v.Id == volunteerId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(v => v.TotalOffersReceived,
                        v => v.TotalOffersReceived + 1));
        }

        public void IncrementTotalOffersAccepted(int volunteerId)
        {
            _context.Volunteers
                .Where(v => v.Id == volunteerId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(v => v.TotalOffersAccepted,
                        v => v.TotalOffersAccepted + 1));
        }

        public List<Volunteer> GetAllWithQualifications()
        {
            return _context.Volunteers
                .Include(v => v.VolunteerHelpTypes)
                .Include(v => v.VolunteerEquipments)
                .Where(v => v.IsActive)
                .ToList();
        }

        public void DecreaseReliabilityScore(int volunteerId)
        {
            _context.Volunteers
                .Where(v => v.Id == volunteerId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        v => v.ReliabilityScore,
                        v => (v.ReliabilityScore <= 0
                            ? 0
                            : v.ReliabilityScore > 100
                                ? 100
                                : v.ReliabilityScore) *
                             (1 - (10.0 / (v.TotalOffersReceived + 1) / 100))));
        }

        public void IncreaseReliabilityScore(int volunteerId)
        {
            _context.Volunteers
                .Where(v => v.Id == volunteerId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        v => v.ReliabilityScore,
                        v => v.ReliabilityScore <= 0
                            ? 0
                            : v.ReliabilityScore >= 100
                                ? 100
                                : v.ReliabilityScore +
                                  (100 - v.ReliabilityScore) *
                                  (10.0 / (v.TotalOffersReceived + 1) / 100)));
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }

        public void AddWithEquipmentAndHelpTypes(
            Volunteer volunteer,
            List<int> equipmentTypeIds,
            List<int> helpTypeIds)
        {
            _context.Volunteers.Add(volunteer);

            foreach (var equipmentTypeId in equipmentTypeIds.Distinct())
            {
                volunteer.VolunteerEquipments.Add(
                    new VolunteerEquipment
                    {
                        Volunteer = volunteer,
                        EquipmentTypeId = equipmentTypeId
                    });
            }

            foreach (var helpTypeId in helpTypeIds.Distinct())
            {
                volunteer.VolunteerHelpTypes.Add(
                    new VolunteerHelpType
                    {
                        Volunteer = volunteer,
                        HelpTypeId = helpTypeId
                    });
            }

            _context.SaveChanges();
        }
    }
}
