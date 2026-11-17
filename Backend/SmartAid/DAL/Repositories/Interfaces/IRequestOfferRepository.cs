
using System;
using System.Collections.Generic;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IRequestOfferRepository
    {
        List<RequestOffer> GetAll();

        RequestOffer? GetById(int id);

        List<RequestOffer> GetPendingByVolunteerId(int volunteerId);

        bool TryAddIfCapacityAvailable(
            RequestOffer requestOffer,
            int maxActiveOffers);

        bool TryDeclineOffer(int offerId, int volunteerId);

        List<int> GetRequestIdsWithExpiredPendingOffers(
            DateTime expirationTime);

        int ExpirePendingOffersForRequest(
            int helpRequestId,
            DateTime expirationTime);

        List<int> ExpirePendingOffers(DateTime expirationTime);
    }
}
