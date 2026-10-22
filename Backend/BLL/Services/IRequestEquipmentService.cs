using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services;

namespace BLL.Services
{
    public interface IRequestEquipmentService
    {
        List<RequestEquipmentDto> GetAll();
        RequestEquipmentDto? GetById(int id);
        void Add(RequestEquipmentDto dto);
        void Update(int id, RequestEquipmentDto dto);
        void Delete(int id);
    }
}
