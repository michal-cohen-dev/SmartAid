
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public bool TryAddIfCapacityAvailable(
            RequestOffer requestOffer,
            int maxActiveOffers)
        {
            using var transaction = _context.Database.BeginTransaction(
                IsolationLevel.Serializable);

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

            bool alreadyAccepted = _context.RequestOffers
                .Any(ro =>
                    ro.HelpRequestId == requestOffer.HelpRequestId &&
                    ro.Status == OfferStatus.Accepted);

            if (alreadyAccepted)
            {
                transaction.Rollback();
                return false;
            }

            bool alreadyOffered = _context.RequestOffers
                .Any(ro =>
                    ro.HelpRequestId == requestOffer.HelpRequestId &&
                    ro.VolunteerId == requestOffer.VolunteerId);

            if (alreadyOffered)
            {
                transaction.Rollback();
                return false;
            }

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

            var initialOffer = _context.RequestOffers
                .AsNoTracking()
                .FirstOrDefault(ro => ro.Id == offerId);

            if (initialOffer == null)
            {
                transaction.Rollback();
                return false;
            }

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

        public List<int> GetRequestIdsWithExpiredPendingOffers(
            DateTime expirationTime)
        {
            return _context.RequestOffers
                .Where(ro =>
                    ro.Status == OfferStatus.Pending &&
                    ro.OfferedAt <= expirationTime &&
                    ro.HelpRequest.Status == RequestStatus.Open &&
                    ro.HelpRequest.AssignedVolunteerId == null)
                .Select(ro => ro.HelpRequestId)
                .Distinct()
                .ToList();
        }

        public int ExpirePendingOffersForRequest(
            int helpRequestId,
            DateTime expirationTime)
        {
            return _context.RequestOffers
                .Where(ro =>
                    ro.HelpRequestId == helpRequestId &&
                    ro.Status == OfferStatus.Pending &&
                    ro.OfferedAt <= expirationTime &&
                    ro.HelpRequest.Status == RequestStatus.Open &&
                    ro.HelpRequest.AssignedVolunteerId == null)
                .ExecuteUpdate(setters =>
                    setters.SetProperty(
                        ro => ro.Status,
                        OfferStatus.Expired));
        }

        public List<int> ExpirePendingOffers(DateTime expirationTime)
        {
            var expiredRequestIds = _context.RequestOffers
                .Where(ro =>
                    ro.Status == OfferStatus.Pending &&
                    ro.OfferedAt <= expirationTime)
                .Select(ro => ro.HelpRequestId)
                .Distinct()
                .ToList();

            if (expiredRequestIds.Count == 0)
                return expiredRequestIds;

            _context.RequestOffers
                .Where(ro =>
                    ro.Status == OfferStatus.Pending &&
                    ro.OfferedAt <= expirationTime)
                .ExecuteUpdate(setters =>
                    setters.SetProperty(
                        ro => ro.Status,
                        OfferStatus.Expired));

            return expiredRequestIds;
        }

        public List<int> ExpirePendingOffersForVolunteerExceptRequest(
            int volunteerId,
            int acceptedRequestId)
        {
            using var transaction = _context.Database.BeginTransaction(
                IsolationLevel.Serializable);

            var offersToExpire = _context.RequestOffers
                .Where(ro =>
                    ro.VolunteerId == volunteerId &&
                    ro.Status == OfferStatus.Pending &&
                    ro.HelpRequestId != acceptedRequestId &&
                    ro.HelpRequest.Status == RequestStatus.Open &&
                    ro.HelpRequest.AssignedVolunteerId == null)
                .ToList();

            var affectedRequestIds = offersToExpire
                .Select(ro => ro.HelpRequestId)
                .Distinct()
                .ToList();

            foreach (var offer in offersToExpire)
            {
                offer.Status = OfferStatus.Expired;
            }

            if (offersToExpire.Count > 0)
                _context.SaveChanges();

            transaction.Commit();

            return affectedRequestIds;
        }
    }
}
