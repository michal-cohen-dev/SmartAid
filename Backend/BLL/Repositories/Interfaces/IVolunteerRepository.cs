using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL.Repositories.Interfaces
{
    public interface IVolunteerRepository
    {
        List<VolunteerDto> GetAll();
        VolunteerDto GetById(int id);
        void Add(VolunteerDto volunteerDto);
        void Update(VolunteerDto volunteerDto);
        void Delete(int id);
    }
}
