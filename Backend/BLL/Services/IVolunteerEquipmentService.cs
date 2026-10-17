
using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services;

namespace BLL.Services
{
    public interface IVolunteerEquipmentService
    {
        List<VolunteerEquipmentDto> GetAll();
        VolunteerEquipmentDto? GetById(int id);
        void Add(VolunteerEquipmentDto dto);
        void Update(int id, VolunteerEquipmentDto dto);
        void Delete(int id);
    }
}
