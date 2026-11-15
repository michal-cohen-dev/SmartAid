using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IHelpTypeRepository
    {
        List<HelpType> GetAll();
        HelpType? GetById(int id);
        void Add(HelpType helpType);
        void Update(HelpType helpType);
        void Delete(int id);
    }
}