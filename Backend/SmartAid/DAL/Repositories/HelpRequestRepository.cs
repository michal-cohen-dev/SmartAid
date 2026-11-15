
using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
            // פתיחת טרנזקציה כדי למנוע קבלת שתי משימות במקביל
            using var transaction =
                _context.Database.BeginTransaction(IsolationLevel.Serializable);

            // בדיקה האם למתנדב כבר יש משימה פעילה
            bool hasActiveAssignment = _context.HelpRequests.Any(request =>
                request.AssignedVolunteerId == volunteerId &&
                (request.Status == RequestStatus.Assigned ||
                 request.Status == RequestStatus.InTreatment));

            if (hasActiveAssignment)
                return false;

            // הקצאת המשימה רק אם היא עדיין פתוחה
            var rowsAffected = _context.HelpRequests
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

            if (rowsAffected != 1)
                return false;

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
