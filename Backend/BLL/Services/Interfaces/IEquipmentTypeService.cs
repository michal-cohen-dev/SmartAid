using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;

namespace BLL.Services.Interfaces
{
    public interface IEquipmentTypeService
    {
        List<EquipmentTypeDto> GetAll();
        EquipmentTypeDto? GetById(int id);
        void Add(EquipmentTypeDto equipmentTypeDto);
        void Update(int id, EquipmentTypeDto equipmentTypeDto);
        void Delete(int id);
    }
}
