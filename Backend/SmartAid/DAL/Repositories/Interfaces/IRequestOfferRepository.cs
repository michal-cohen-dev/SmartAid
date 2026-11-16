
using System.Collections.Generic;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IRequestOfferRepository
    {
        List<RequestOffer> GetAll();

        RequestOffer? GetById(int id);

        List<RequestOffer> GetPendingByVolunteerId(int volunteerId);

        void Add(RequestOffer requestOffer);

        bool TryAddIfCapacityAvailable(
            RequestOffer requestOffer,
            int maxActiveOffers);

        bool TryDeclineOffer(int offerId, int volunteerId);

        void Update(RequestOffer requestOffer);

        void Delete(int id);

        int ExpirePendingOffers(DateTime expirationTime);
    }
}
