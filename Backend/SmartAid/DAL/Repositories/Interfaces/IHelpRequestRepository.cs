using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Interfaces
{
    public interface IHelpRequestRepository
    {
        List<HelpRequest> GetAll();
        HelpRequest? GetById(int id);

        // שליפה רק אם הבקשה שייכת למשתמש
        HelpRequest? GetOwned(int id, int userId);

        void Add(HelpRequest helpRequest);
        void Update(HelpRequest helpRequest);
        void Delete(int id);
    }
}