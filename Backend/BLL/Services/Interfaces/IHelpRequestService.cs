
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DTO;

namespace BLL.Services.Interfaces
{
    public interface IHelpRequestService
    {
        List<HelpRequestDto> GetAll();
        HelpRequestDto? GetById(int id);

        Task<List<HelpRequestPreviewDto>> GetOpenForVolunteerAsync(
            int userId,
            CancellationToken cancellationToken = default);

        List<HelpRequestDto> GetByUserId(int userId);
        List<HelpRequestDto> GetByVolunteerId(int userId);

        Task<int> Add(
            HelpRequestPostDto helpRequestDto,
            int userId,
            CancellationToken cancellationToken = default);

        bool Update(int id, HelpRequestUpdateDto helpRequestDto, int userId);
        bool Delete(int id, int userId);
        bool IsOwner(int requestId, int userId);
        bool IsAssignedVolunteer(int requestId, int userId);
        bool TryAssignToVolunteer(int requestId, int userId);
        bool TryMarkInTreatment(int requestId, int userId);
        bool TryCompleteRequest(int requestId, int userId);
        bool TryRateCompletedRequest(int requestId, int userId, int rating);
        bool TryCancelRequest(int requestId, int userId);
    }
}
