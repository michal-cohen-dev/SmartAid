using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;

namespace BLL.Services.Interfaces
{
    public interface IVolunteerService
    {
        List<VolunteerDto> GetAll();
        VolunteerDto? GetById(int id);
        void Add(VolunteerDto volunteerDto);
        void Update(int id, VolunteerDto volunteerDto);
        void Delete(int id);
    }
}

