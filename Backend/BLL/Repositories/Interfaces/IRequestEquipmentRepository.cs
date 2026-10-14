using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL.Repositories.Interfaces
{
    public interface IRequestEquipmentRepository
    {
        List<RequestEquipmentDto> GetAll();
        RequestEquipmentDto GetById(int id);
        void Add(RequestEquipmentDto requestEquipmentDto);
        void Update(RequestEquipmentDto requestEquipmentDto);
        void Delete(int id);
    }
}
