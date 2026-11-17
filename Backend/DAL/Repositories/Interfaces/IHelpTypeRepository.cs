
using DAL.Entities;
using System.Collections.Generic;

namespace DAL.Repositories.Interfaces
{
    public interface IHelpTypeRepository
    {
        List<HelpType> GetAll();
        HelpType? GetById(int id);
        bool IsInUse(int id);
        void Add(HelpType helpType);
        void Update(HelpType helpType);
        void Delete(int id);
    }
}
