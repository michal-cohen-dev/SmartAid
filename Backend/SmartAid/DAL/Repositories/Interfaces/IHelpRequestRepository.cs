
using System.Collections.Generic;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IHelpRequestRepository
    {
        List<HelpRequest> GetAll();
        List<HelpRequest> GetOpenRequests();
        List<HelpRequest> GetByUserId(int userId);

        HelpRequest? GetById(int id);
        HelpRequest? GetOwned(int id, int userId);

        bool TryAssignToVolunteer(int requestId, int volunteerId);

        void Add(HelpRequest helpRequest);
        void Update(HelpRequest helpRequest);
        void Delete(int id);
    }
}
