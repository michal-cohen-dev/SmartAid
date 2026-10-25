using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;

namespace BLL.Services.Interfaces
{
    public interface IHelpRequestService
    {
        List<HelpRequestDto> GetAll();
        HelpRequestDto? GetById(int id);
        void Add(HelpRequestPostDto helpRequestDto);
        void Update(int id, HelpRequestDto helpRequestDto);
        void Delete(int id);
    }
}
