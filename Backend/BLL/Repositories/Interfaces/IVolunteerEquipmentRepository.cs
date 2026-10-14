using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL.Repositories.Interfaces
{
    public interface IVolunteerEquipmentRepository
    {
        List<VolunteerEquipmentDto> GetAll();
        VolunteerEquipmentDto GetById(int id);
        void Add(VolunteerEquipmentDto volunteerEquipmentDto);
        void Update(VolunteerEquipmentDto volunteerEquipmentDto);
        void Delete(int id);
    }
}
