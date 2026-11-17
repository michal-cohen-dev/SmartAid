
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DTO;

namespace BLL.Services.Interfaces
{
    public interface IRequestOfferService
    {
        List<RequestOfferDto> GetAll();

        RequestOfferDto? GetById(int id);

        List<RequestOfferDto> GetPendingByVolunteerId(int volunteerId);

        void Add(RequestOfferDto requestOfferDto);

        void Update(int id, RequestOfferDto requestOfferDto);

        Task<bool> TryDeclineOfferAsync(
            int offerId,
            int userId,
            CancellationToken cancellationToken = default);

        Task<int> CreateOffersForRequestAsync(
            int helpRequestId,
            CancellationToken cancellationToken = default,
            bool onlyNeverOffered = false);

        Task<bool> CreateOfferForVolunteerAsync(
            int helpRequestId,
            int volunteerId,
            CancellationToken cancellationToken = default);

        Task ReassignPendingOffersAfterVolunteerAssignmentAsync(
            int volunteerId,
            int acceptedRequestId,
            CancellationToken cancellationToken = default);
    }
}
