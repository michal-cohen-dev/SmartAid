
using System.Collections.Generic;
using System.Linq;
using System.Data;
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
                .ToList();
        }

        public List<HelpRequest> GetByUserId(int userId)
        {
            return _context.HelpRequests
                .Include(hr => hr.User)
                .Include(hr => hr.AssignedVolunteer)
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

        public bool TryAssignToVolunteer(int requestId, int volunteerId)
        {
            using var transaction =
                _context.Database.BeginTransaction(
                    IsolationLevel.Serializable);

            // Lock the request row.
            // This synchronizes assignment with offer creation.
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

            // Lock the volunteer row so that simultaneous
            // assignments to the same volunteer are serialized.
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

            // The volunteer must have an active pending offer
            // for this specific request.
            bool hasPendingOffer = _context.RequestOffers
    .Any(offer =>
        offer.HelpRequestId == requestId &&
        offer.VolunteerId == volunteerId &&
        offer.Status == OfferStatus.Pending &&
        offer.OfferedAt > DateTime.UtcNow.AddMinutes(-2));

            if (!hasPendingOffer)
            {
                transaction.Rollback();
                return false;
            }

            // Do not allow a volunteer to receive another
            // active assignment.
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

            // Do not assign a request that already has
            // an accepted offer.
            bool alreadyAccepted = _context.RequestOffers
                .Any(offer =>
                    offer.HelpRequestId == requestId &&
                    offer.Status == OfferStatus.Accepted);

            if (alreadyAccepted)
            {
                transaction.Rollback();
                return false;
            }

            // Assign the request atomically.
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

            // Mark the accepted volunteer's offer as Accepted.
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

            // Expire all other pending offers for this request.
            _context.RequestOffers
                .Where(offer =>
                    offer.HelpRequestId == requestId &&
                    offer.VolunteerId != volunteerId &&
                    offer.Status == OfferStatus.Pending)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        offer => offer.Status,
                        OfferStatus.Expired));

            // Update the accepted-offer counter.
            _context.Volunteers
                .Where(v => v.Id == volunteerId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(
                        v => v.TotalOffersAccepted,
                        v => v.TotalOffersAccepted + 1));

            transaction.Commit();

            return true;
        }

        public bool TryMarkInTreatment(int requestId, int volunteerId)
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

        public bool TryCompleteRequest(int requestId, int volunteerId)
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

        public bool TryCancelRequest(
            int requestId,
            int userId,
            bool cancelledByVolunteer)
        {
            var query = _context.HelpRequests
                .Where(request => request.Id == requestId);

            if (cancelledByVolunteer)
            {
                query = query.Where(request =>
                    request.AssignedVolunteer != null &&
                    request.AssignedVolunteer.UserId == userId &&
                    (request.Status == RequestStatus.Assigned ||
                     request.Status == RequestStatus.InTreatment));
            }
            else
            {
                query = query.Where(request =>
                    request.UserId == userId &&
                    (request.Status == RequestStatus.Open ||
                     request.Status == RequestStatus.Assigned));
            }

            var rowsAffected = query.ExecuteUpdate(setters => setters
                .SetProperty(
                    request => request.Status,
                    RequestStatus.Cancelled));

            return rowsAffected == 1;
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
