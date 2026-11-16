using System.Collections.Generic;
using DAL.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.Repositories.Interfaces
{
    public interface IVolunteerRepository
    {
        List<Volunteer> GetAll();

        Volunteer? GetById(int id);

        Volunteer? GetByUserId(int userId);

        Volunteer? GetByIdWithQualifications(int id);

        Volunteer? GetByUserIdWithQualifications(int userId);

        List<Volunteer> GetAllWithQualifications();

        void Add(Volunteer volunteer);

        void Update(Volunteer volunteer);

        void Delete(int id);

        void IncrementTotalCompleted(int volunteerId);

        void IncrementTotalOffersReceived(int volunteerId);

        void IncrementTotalOffersAccepted(int volunteerId);

        void DecreaseReliabilityScore(int volunteerId);

        void IncreaseReliabilityScore(int volunteerId);

        IDbContextTransaction BeginTransaction();

        void AddWithEquipmentAndHelpTypes(
            Volunteer volunteer,
            List<int> equipmentTypeIds,
            List<int> helpTypeIds);

        void ReplaceQualifications(
            int volunteerId,
            List<int> equipmentTypeIds,
            List<int> helpTypeIds);
    }
}