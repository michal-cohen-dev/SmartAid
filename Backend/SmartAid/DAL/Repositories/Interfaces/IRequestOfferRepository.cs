using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Interfaces
{
    public interface IRequestOfferRepository
    {
        List<RequestOffer> GetAll();
        RequestOffer? GetById(int id);
        void Add(RequestOffer requestOffer);
        void Update(RequestOffer requestOffer);
        void Delete(int id);
    }
}