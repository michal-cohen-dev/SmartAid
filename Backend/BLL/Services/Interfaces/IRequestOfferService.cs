
using DTO;

namespace BLL.Services.Interfaces
{
    public interface IRequestOfferService
    {
        List<RequestOfferDto> GetAll();

        RequestOfferDto? GetById(int id);

        void Add(RequestOfferDto requestOfferDto);

        void Update(int id, RequestOfferDto requestOfferDto);

        bool TryDeclineOffer(int offerId, int userId);

        void Delete(int id);
    }
}
