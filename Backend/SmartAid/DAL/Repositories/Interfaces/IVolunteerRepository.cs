
using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Interfaces
{

    public interface IVolunteerRepository
    {
        List<Volunteer> GetAll();
        Volunteer? GetById(int id);
        void Add(Volunteer volunteer);
        void Update(Volunteer volunteer);
        void Delete(int id);
    }
}
