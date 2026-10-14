using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL.Repositories.Interfaces
{
    public interface IEquipmentTypeRepository
    {
        List<EquipmentTypeDto> GetAll();
        EquipmentTypeDto GetById(int id);
        void Add(EquipmentTypeDto equipmentTypeDto);
        void Update(EquipmentTypeDto equipmentTypeDto);
        void Delete(int id);
    }
}
