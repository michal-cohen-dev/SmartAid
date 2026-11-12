
using DTO;

namespace BLL.Services.Interfaces
{
    public interface IHelpRequestService
    {
        List<HelpRequestDto> GetAll();
        HelpRequestDto? GetById(int id);
        List<HelpRequestPreviewDto> GetOpenForVolunteer(int userId);
        List<HelpRequestDto> GetByUserId(int userId);
        List<HelpRequestDto> GetByVolunteerId(int userId);

        int Add(HelpRequestPostDto helpRequestDto, int userId);
        bool Update(int id, HelpRequestUpdateDto helpRequestDto, int userId);
        bool Delete(int id, int userId);
        bool IsOwner(int requestId, int userId);
        bool IsAssignedVolunteer(int requestId, int userId);
        bool TryAssignToVolunteer(int requestId, int userId);
        bool TryMarkInTreatment(int requestId, int userId);
        bool TryCompleteRequest(int requestId, int userId);
    }
}
