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
        void Add(HelpRequest helpRequest);
        void Update(HelpRequest helpRequest);
        void Delete(int id);
    }
}