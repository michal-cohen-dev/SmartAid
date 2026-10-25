using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;

namespace BLL.Services.Interfaces
{
    public interface IRequestOfferService
    {
        List<RequestOfferDto> GetAll();
        RequestOfferDto? GetById(int id);
        void Add(RequestOfferDto requestOfferDto);
        void Update(int id, RequestOfferDto requestOfferDto);
        void Delete(int id);
    }
}
