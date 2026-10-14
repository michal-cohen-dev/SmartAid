using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BLL.Repositories.Interfaces;
using DAL;
using DAL.Entities;
using DTO;

namespace BLL.Repositories
{
    public class RequestOfferRepository : IRequestOfferRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public RequestOfferRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<RequestOfferDto> GetAll()
        {
            return _context.RequestOffers
                .ProjectTo<RequestOfferDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public RequestOfferDto GetById(int id)
        {
            return _context.RequestOffers
                .Where(ro => ro.Id == id)
                .ProjectTo<RequestOfferDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault()!;
        }

        public void Add(RequestOfferDto requestOfferDto)
        {
            var entity = _mapper.Map<RequestOffer>(requestOfferDto);
            entity.OfferedAt = DateTime.Now;
            _context.RequestOffers.Add(entity);
            _context.SaveChanges(); // עבודה סינכרונית בלבד
        }

        public void Update(RequestOfferDto requestOfferDto)
        {
            var entity = _context.RequestOffers.Find(requestOfferDto.Id);
            if (entity != null)
            {
                entity.Status = (OfferStatus)requestOfferDto.Status;
                _context.RequestOffers.Update(entity);
                _context.SaveChanges();
            }
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
