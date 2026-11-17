
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class HelpRequestRepository : IHelpRequestRepository
    {
        private readonly AppDbContext _context;

        public HelpRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<HelpRequest> GetAll()
        {
            return _context.HelpRequests
                .Include(hr => hr.User)
                .Include(hr => hr.AssignedVolunteer)
                .Include(hr => hr.HelpType)
                .Include(hr => hr.RequiredEquipments)
                    .ThenInclude(re => re.EquipmentType)
                .ToList();
        }

        public List<HelpRequest> GetByUserId(int userId)
        {
            return _context.HelpRequests
                .Include(hr => hr.User)
                .Include(hr => hr.AssignedVolunteer)
                .Include(hr => hr.HelpType)
                .Include(hr => hr.RequiredEquipments)
                    .ThenInclude(re => re.EquipmentType)
                .Where(hr => hr.UserId == userId)
                .OrderByDescending(hr => hr.CreatedAt)
                .ToList();
        }

        public List<HelpRequest> GetByVolunteerId(int volunteerId)
        {
            return _context.HelpRequests
                .Include(hr => hr.User)
                .Include(hr => hr.AssignedVolunteer)
                .Include(hr => hr.HelpType)
                .Include(hr => hr.RequiredEquipments)
                    .ThenInclude(re => re.EquipmentType)
                .Where(hr => hr.AssignedVolunteerId == volunteerId)
                .OrderByDescending(hr => hr.CreatedAt)
                .ToList();
        }

        public HelpRequest? GetById(int id)
        {
            return _context.HelpRequests
                .Include(hr => hr.User)
                .Include(hr => hr.AssignedVolunteer)
                .Include(hr => hr.HelpType)
                .Include(hr => hr.RequiredEquipments)
                    .ThenInclude(re => re.EquipmentType)
                .FirstOrDefault(hr => hr.Id == id);
        }

        public HelpRequest? GetOwned(int id, int userId)
        {
            return _context.HelpRequests
                .Include(hr => hr.User)
                .Include(hr => hr.AssignedVolunteer)
                .FirstOrDefault(hr =>
                    hr.Id == id && hr.UserId == userId);
        }

        public void Add(HelpRequest entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            _context.HelpRequests.Add(entity);
            _context.SaveChanges();
        }

        public void AddWithEquipment(
            HelpRequest entity,
            List<int> equipmentTypeIds)
        {
            entity.CreatedAt = DateTime.UtcNow;

            entity.RequiredEquipments = equipmentTypeIds
                .Distinct()
                .Select(equipmentTypeId => new RequestEquipment
                {
                    EquipmentTypeId = equipmentTypeId
                })
                .ToList();

            _context.HelpRequests.Add(entity);
            _context.SaveChanges();
        }

        public bool TryUpdateWithEquipment(
            HelpRequest entity,
            int userId,
            List<int> equipmentTypeIds)
        {
            using var transaction =
                _context.Database.BeginTransaction(
                    IsolationLevel.Serializable);

            var currentRequest = _context.HelpRequests
                .FromSqlInterpolated(
                    $"SELECT * FROM HelpRequests WITH (UPDLOCK, HOLDLOCK) WHERE Id = {entity.Id}")
                .AsNoTracking()
                .FirstOrDefault();

            if (currentRequest == null ||
                currentRequest.UserId != userId ||
                currentRequest.Status != RequestStatus.Open ||
                currentRequest.AssignedVolunteerId != null)
            {
                transaction.Rollback();
                return false;
            }

            var existingEquipments = _context.RequestEquipments
                .Where(equipment =>
                    equipment.HelpRequestId == entity.Id)
                .ToList();

            _context.RequestEquipments.RemoveRange(existingEquipments);

            var newEquipments = equipmentTypeIds
                .Distinct()
                .Select(equipmentTypeId => new RequestEquipment
                {
                    HelpRequestId = entity.Id,
                    EquipmentTypeId = equipmentTypeId
                })
                .ToList();

            _context.RequestEquipments.AddRange(newEquipments);

            _context.SaveChanges();

            transaction.Commit();

            return true;
        }

        public void Update(HelpRequest entity)
        {
            _context.HelpRequests.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.HelpRequests.Find(id);

            if (entity != null)
            {
                _context.HelpRequests.Remove(entity);
                _context.SaveChanges();
            }
        }

        public List<HelpRequest> GetOpenRequests()
        {
            return _context.HelpRequests
                .Include(hr => hr.User)
                .Include(hr => hr.RequiredEquipments)
                    .ThenInclude(re => re.EquipmentType)
                .Where(hr =>
                    hr.Status == RequestStatus.Open &&
                    hr.AssignedVolunteerId == null)
                .ToList();
        }

        public bool TryAssignToVolunteer(
    int requestId,
    int volunteerId,
    bool allowExpiredOffer)
        {
            using var transaction =
                _context.Database.BeginTransaction(
                    IsolationLevel.Serializable);

            var helpRequest = _context.HelpRequests
                .FromSqlInterpolated(
                    $"SELECT * FROM HelpRequests WITH (UPDLOCK, HOLDLOCK) WHERE Id = {requestId}")
                .AsNoTracking()
                .FirstOrDefault();

            if (helpRequest == null ||
                helpRequest.Status != RequestStatus.Open ||
                helpRequest.AssignedVolunteerId != null)
            {
                transaction.Rollback();
                return false;
            }

            var volunteer = _context.Volunteers
                .FromSqlInterpolated(
                    $"SELECT * FROM Volunteers WITH (UPDLOCK, HOLDLOCK) WHERE Id = {volunteerId}")
                .AsNoTracking()
                .FirstOrDefault();

            if (volunteer == null || !volunteer.IsActive)
            {
                transaction.Rollback();
                return false;
            }

            // ההצעה חייבת להיות ממתינה.
            // אם עברו שתי דקות, נבדוק האם מותר לקבל אותה באיחור.
            bool hasPendingOffer = _context.RequestOffers
                .Any(offer =>
                    offer.HelpRequestId == requestId &&
                    offer.VolunteerId == volunteerId &&
                    offer.Status == OfferStatus.Pending &&
                    (
                        allowExpiredOffer ||
                        offer.OfferedAt > DateTime.UtcNow.AddMinutes(-2)
                    ));

            if (!hasPendingOffer)
            {
                transaction.Rollback();
                return false;
            }

            bool hasActiveAssignment = _context.HelpRequests
                .Any(request =>
                    request.AssignedVolunteerId == volunteerId &&
                    (request.Status == RequestStatus.Assigned ||
                     request.Status == RequestStatus.InTreatment));

            if (hasActiveAssignment)
            {
                transaction.Rollback();
                return false;
            }

            bool alreadyAccepted = _context.RequestOffers
                .Any(offer =>
                    offer.HelpRequestId == requestId &&
                    offer.Status == OfferStatus.Accepted);

            if (alreadyAccepted)
            {
                transaction.Rollback();
                return false;
            }

            int requestRowsAffected = _context.HelpRequests
                .Where(request =>
                    request.Id == requestId &&
                    request.Status == RequestStatus.Open &&
                    request.AssignedVolunteerId == null)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        request => request.AssignedVolunteerId,
                        volunteerId)
                    .SetProperty(
                        request => request.Status,
                        RequestStatus.Assigned));

            if (requestRowsAffected != 1)
            {
                transaction.Rollback();
                return false;
            }

            int acceptedRowsAffected = _context.RequestOffers
                .Where(offer =>
                    offer.HelpRequestId == requestId &&
                    offer.VolunteerId == volunteerId &&
                    offer.Status == OfferStatus.Pending)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        offer => offer.Status,
                        OfferStatus.Accepted));

            if (acceptedRowsAffected != 1)
            {
                transaction.Rollback();
                return false;
            }

            _context.RequestOffers
                .Where(offer =>
                    offer.HelpRequestId == requestId &&
                    offer.VolunteerId != volunteerId &&
                    offer.Status == OfferStatus.Pending)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        offer => offer.Status,
                        OfferStatus.Expired));

            _context.Volunteers
                .Where(v => v.Id == volunteerId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        v => v.TotalOffersAccepted,
                        v => v.TotalOffersAccepted + 1));

            transaction.Commit();

            return true;
        }

        public bool TryMarkInTreatment(
            int requestId,
            int volunteerId)
        {
            var rowsAffected = _context.HelpRequests
                .Where(request =>
                    request.Id == requestId &&
                    request.Status == RequestStatus.Assigned &&
                    request.AssignedVolunteerId == volunteerId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        request => request.Status,
                        RequestStatus.InTreatment));

            return rowsAffected == 1;
        }

        public bool TryCompleteRequest(
            int requestId,
            int volunteerId)
        {
            var rowsAffected = _context.HelpRequests
                .Where(request =>
                    request.Id == requestId &&
                    request.Status == RequestStatus.InTreatment &&
                    request.AssignedVolunteerId == volunteerId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        request => request.Status,
                        RequestStatus.Completed));

            return rowsAffected == 1;
        }

        public bool TryRateCompletedRequest(
            int requestId,
            int userId,
            int rating)
        {
            var rowsAffected = _context.HelpRequests
                .Where(request =>
                    request.Id == requestId &&
                    request.UserId == userId &&
                    request.Status == RequestStatus.Completed &&
                    request.Rating == null)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        request => request.Rating,
                        rating));

            return rowsAffected == 1;
        }

        public bool TryAdminCancelRequest(int requestId)
        {
            using var transaction =
                _context.Database.BeginTransaction(
                    IsolationLevel.Serializable);

            var request = _context.HelpRequests
                .FromSqlInterpolated(
                    $"SELECT * FROM HelpRequests WITH (UPDLOCK, HOLDLOCK) WHERE Id = {requestId}")
                .AsNoTracking()
                .FirstOrDefault();

            if (request == null ||
                (request.Status != RequestStatus.Open &&
                 request.Status != RequestStatus.Assigned &&
                 request.Status != RequestStatus.InTreatment))
            {
                transaction.Rollback();
                return false;
            }

            int rowsAffected = _context.HelpRequests
                .Where(r =>
                    r.Id == requestId &&
                    (r.Status == RequestStatus.Open ||
                     r.Status == RequestStatus.Assigned ||
                     r.Status == RequestStatus.InTreatment))
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        r => r.Status,
                        RequestStatus.Cancelled));

            if (rowsAffected != 1)
            {
                transaction.Rollback();
                return false;
            }

            _context.RequestOffers
                .Where(offer =>
                    offer.HelpRequestId == requestId &&
                    offer.Status == OfferStatus.Pending)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        offer => offer.Status,
                        OfferStatus.Expired));

            transaction.Commit();

            return true;
        }

        public bool TryCancelRequest(
            int requestId,
            int userId,
            bool cancelledByVolunteer)
        {
            bool ownsTransaction =
                _context.Database.CurrentTransaction == null;

            using var transaction = ownsTransaction
                ? _context.Database.BeginTransaction(
                    IsolationLevel.Serializable)
                : null;

            var request = _context.HelpRequests
                .FromSqlInterpolated(
                    $"SELECT * FROM HelpRequests WITH (UPDLOCK, HOLDLOCK) WHERE Id = {requestId}")
                .AsNoTracking()
                .FirstOrDefault();

            if (request == null)
            {
                transaction?.Rollback();
                return false;
            }

            bool canCancel;

            if (cancelledByVolunteer)
            {
                canCancel = _context.HelpRequests
                    .Any(r =>
                        r.Id == requestId &&
                        r.AssignedVolunteer != null &&
                        r.AssignedVolunteer.UserId == userId &&
                        (r.Status == RequestStatus.Assigned ||
                         r.Status == RequestStatus.InTreatment));
            }
            else
            {
                canCancel =
                    request.UserId == userId &&
                    (request.Status == RequestStatus.Open ||
                     request.Status == RequestStatus.Assigned);
            }

            if (!canCancel)
            {
                transaction?.Rollback();
                return false;
            }

            int rowsAffected;

            if (cancelledByVolunteer)
            {
                rowsAffected = _context.HelpRequests
                    .Where(r =>
                        r.Id == requestId &&
                        r.AssignedVolunteerId != null)
                    .ExecuteUpdate(setters => setters
                        .SetProperty(
                            r => r.Status,
                            RequestStatus.Open)
                        .SetProperty(
                            r => r.AssignedVolunteerId,
                            (int?)null));
            }
            else
            {
                rowsAffected = _context.HelpRequests
                    .Where(r => r.Id == requestId)
                    .ExecuteUpdate(setters => setters
                        .SetProperty(
                            r => r.Status,
                            RequestStatus.Cancelled));
            }

            if (rowsAffected != 1)
            {
                transaction?.Rollback();
                return false;
            }

            _context.RequestOffers
                .Where(offer =>
                    offer.HelpRequestId == requestId &&
                    offer.Status == OfferStatus.Pending)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        offer => offer.Status,
                        OfferStatus.Expired));

            transaction?.Commit();

            return true;
        }

        public bool HasActiveAssignment(int volunteerId)
        {
            return _context.HelpRequests.Any(request =>
                request.AssignedVolunteerId == volunteerId &&
                (request.Status == RequestStatus.Assigned ||
                 request.Status == RequestStatus.InTreatment));
        }

        public List<int> GetRatingsByVolunteerId(int volunteerId)
        {
            return _context.HelpRequests
                .Where(hr =>
                    hr.AssignedVolunteerId == volunteerId &&
                    hr.Status == RequestStatus.Completed &&
                    hr.Rating != null)
                .Select(hr => hr.Rating!.Value)
                .ToList();
        }
    }
}
