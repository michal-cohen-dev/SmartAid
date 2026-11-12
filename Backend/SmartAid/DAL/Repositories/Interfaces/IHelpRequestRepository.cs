
using System.Collections.Generic;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IHelpRequestRepository
    {
        List<HelpRequest> GetAll();
        List<HelpRequest> GetOpenRequests();
        List<HelpRequest> GetByUserId(int userId);
        List<HelpRequest> GetByVolunteerId(int volunteerId);
        HelpRequest? GetById(int id);
        HelpRequest? GetOwned(int id, int userId);

        bool TryAssignToVolunteer(int requestId, int volunteerId);
        bool TryMarkInTreatment(int requestId, int volunteerId);
        bool TryCompleteRequest(int requestId, int volunteerId);
        void Add(HelpRequest helpRequest);
        void Update(HelpRequest helpRequest);
        void Delete(int id);
    }
}
