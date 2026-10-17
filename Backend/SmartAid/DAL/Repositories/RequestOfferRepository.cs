using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class RequestOfferRepository : IRequestOfferRepository
    {
        private readonly AppDbContext _context;

        public RequestOfferRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<RequestOffer> GetAll()
        {
            return _context.RequestOffers
                .Include(ro => ro.HelpRequest)
                .Include(ro => ro.Volunteer)
                .ToList();
        }

        public RequestOffer? GetById(int id)
        {
            return _context.RequestOffers
                .Include(ro => ro.HelpRequest)
                .Include(ro => ro.Volunteer)
                .FirstOrDefault(ro => ro.Id == id);
        }

        public void Add(RequestOffer requestOffer)
        {
            requestOffer.OfferedAt = DateTime.UtcNow;
            _context.RequestOffers.Add(requestOffer);
            _context.SaveChanges();
        }

        public void Update(RequestOffer requestOffer)
        {
            _context.RequestOffers.Update(requestOffer);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.RequestOffers.Find(id);
            if (entity != null)
            {
                _context.RequestOffers.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}