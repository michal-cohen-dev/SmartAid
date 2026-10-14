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
using Microsoft.EntityFrameworkCore;

namespace BLL.Repositories
{
    public class RequestEquipmentRepository : IRequestEquipmentRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public RequestEquipmentRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<RequestEquipmentDto> GetAll()
        {
            return _context.RequestEquipments
                .Include(re => re.HelpRequest)
                .Include(re => re.EquipmentType)
                .ProjectTo<RequestEquipmentDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public RequestEquipmentDto GetById(int id)
        {
            return _context.RequestEquipments
                .Where(re => re.Id == id)
                .Include(re => re.HelpRequest)
                .Include(re => re.EquipmentType)
                .ProjectTo<RequestEquipmentDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault()!;
        }

        public void Add(RequestEquipmentDto requestEquipmentDto)
        {
            var entity = _mapper.Map<RequestEquipment>(requestEquipmentDto);
            _context.RequestEquipments.Add(entity);
            _context.SaveChanges(); // עבודה סינכרונית בלבד
        }

        public void Update(RequestEquipmentDto requestEquipmentDto)
        {
            var entity = _context.RequestEquipments.Find(requestEquipmentDto.Id);
            if (entity != null)
            {
                entity.HelpRequestId = requestEquipmentDto.HelpRequestId;
                entity.EquipmentTypeId = requestEquipmentDto.EquipmentTypeId;

                _context.RequestEquipments.Update(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.RequestEquipments.Find(id);
            if (entity != null)
            {
                _context.RequestEquipments.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
