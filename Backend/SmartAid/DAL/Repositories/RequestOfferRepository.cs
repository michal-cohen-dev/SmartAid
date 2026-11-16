
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace DAL.Repositories
{
    public class RequestOfferRepository : IRequestOfferRepository
    {
        private readonly AppDbContext _context;

        public RequestOfferRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<RequestOffer> GetAll()
        {
            return _context.RequestOffers
                .Include(ro => ro.HelpRequest)
                .Include(ro => ro.Volunteer)
                .ToList();
        }

        public RequestOffer? GetById(int id)
        {
            return _context.RequestOffers
                .Include(ro => ro.HelpRequest)
                .Include(ro => ro.Volunteer)
                .FirstOrDefault(ro => ro.Id == id);
        }



        public List<RequestOffer> GetPendingByVolunteerId(int volunteerId)
        {
            return _context.RequestOffers
                .Where(ro =>
                    ro.VolunteerId == volunteerId &&
                    ro.Status == OfferStatus.Pending)
                .ToList();
        }




        public void Add(RequestOffer requestOffer)
        {
            requestOffer.OfferedAt = DateTime.UtcNow;
            _context.RequestOffers.Add(requestOffer);
            _context.SaveChanges();
        }

        public bool TryAddIfCapacityAvailable(
            RequestOffer requestOffer,
            int maxActiveOffers)
        {
            using var transaction = _context.Database.BeginTransaction(
                IsolationLevel.Serializable);

            // Lock the request row so concurrent offer-creation
            // operations for the same request are serialized.
            var helpRequest = _context.HelpRequests
    .FromSqlInterpolated(
        $"SELECT * FROM HelpRequests WITH (UPDLOCK, HOLDLOCK) WHERE Id = {requestOffer.HelpRequestId}")
    .AsNoTracking()
    .FirstOrDefault();

            if (helpRequest == null ||
               helpRequest.Status != RequestStatus.Open ||
                helpRequest.AssignedVolunteerId != null)
            {
                transaction.Rollback();
                return false;
            }

            // Do not create more offers after someone has accepted.
            bool alreadyAccepted = _context.RequestOffers
                .Any(ro =>
                    ro.HelpRequestId == requestOffer.HelpRequestId &&
                    ro.Status == OfferStatus.Accepted);

            if (alreadyAccepted)
            {
                transaction.Rollback();
                return false;
            }

            // A volunteer can receive only one offer per request.
            bool alreadyOffered = _context.RequestOffers
                .Any(ro =>
                    ro.HelpRequestId == requestOffer.HelpRequestId &&
                    ro.VolunteerId == requestOffer.VolunteerId);

            if (alreadyOffered)
            {
                transaction.Rollback();
                return false;
            }

            // Only Pending offers count toward the active-offer limit.
            int activeOffersCount = _context.RequestOffers
                .Count(ro =>
                    ro.HelpRequestId == requestOffer.HelpRequestId &&
                    ro.Status == OfferStatus.Pending);

            if (activeOffersCount >= maxActiveOffers)
            {
                transaction.Rollback();
                return false;
            }

            requestOffer.Status = OfferStatus.Pending;
            requestOffer.OfferedAt = DateTime.UtcNow;

            _context.RequestOffers.Add(requestOffer);
            _context.SaveChanges();

            transaction.Commit();
            return true;
        }



        public bool TryDeclineOffer(int offerId, int volunteerId)
        {
            using var transaction = _context.Database.BeginTransaction(
                IsolationLevel.Serializable);

            // Read the request ID so we can lock the request first.
            var initialOffer = _context.RequestOffers
                .AsNoTracking()
                .FirstOrDefault(ro => ro.Id == offerId);

            if (initialOffer == null)
            {
                transaction.Rollback();
                return false;
            }

            // Lock the help request first, using the same lock order
            // as the assignment operation.
            var helpRequest = _context.HelpRequests
                .FromSqlInterpolated(
                    $"SELECT * FROM HelpRequests WITH (UPDLOCK, HOLDLOCK) WHERE Id = {initialOffer.HelpRequestId}")
                .AsNoTracking()
                .FirstOrDefault();

            if (helpRequest == null ||
                helpRequest.Status != RequestStatus.Open ||
                helpRequest.AssignedVolunteerId != null)
            {
                transaction.Rollback();
                return false;
            }

            // Lock the offer before checking and changing its status.
            var offer = _context.RequestOffers
                .FromSqlInterpolated(
                    $"SELECT * FROM RequestOffers WITH (UPDLOCK, HOLDLOCK) WHERE Id = {offerId}")
                .FirstOrDefault();

            if (offer == null ||
                offer.VolunteerId != volunteerId ||
                offer.Status != OfferStatus.Pending)
            {
                transaction.Rollback();
                return false;
            }

            bool alreadyAccepted = _context.RequestOffers
                .Any(ro =>
                    ro.HelpRequestId == offer.HelpRequestId &&
                    ro.Status == OfferStatus.Accepted);

            if (alreadyAccepted)
            {
                transaction.Rollback();
                return false;
            }

            offer.Status = OfferStatus.Declined;

            _context.SaveChanges();
            transaction.Commit();

            return true;
        }




        public int ExpirePendingOffers(DateTime expirationTime)
        {
            return _context.RequestOffers
                .Where(ro =>
                    ro.Status == OfferStatus.Pending &&
                    ro.OfferedAt <= expirationTime)
                .ExecuteUpdate(setters =>
                    setters.SetProperty(
                        ro => ro.Status,
                        OfferStatus.Expired));
        }



        public void Update(RequestOffer requestOffer)
        {
            _context.RequestOffers.Update(requestOffer);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.RequestOffers.Find(id);

            if (entity != null)
            {
                _context.RequestOffers.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
