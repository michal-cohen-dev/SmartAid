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
    public class VolunteerEquipmentRepository : IVolunteerEquipmentRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public VolunteerEquipmentRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<VolunteerEquipmentDto> GetAll()
        {
            return _context.VolunteerEquipments
                .Include(ve => ve.Volunteer)
                .Include(ve => ve.EquipmentType)
                .ProjectTo<VolunteerEquipmentDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public VolunteerEquipmentDto GetById(int id)
        {
            return _context.VolunteerEquipments
                .Where(ve => ve.Id == id)
                .Include(ve => ve.Volunteer)
                .Include(ve => ve.EquipmentType)
                .ProjectTo<VolunteerEquipmentDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault()!;
        }

        public void Add(VolunteerEquipmentDto volunteerEquipmentDto)
        {
            var entity = _mapper.Map<VolunteerEquipment>(volunteerEquipmentDto);
            _context.VolunteerEquipments.Add(entity);
            _context.SaveChanges();
        }

        public void Update(VolunteerEquipmentDto volunteerEquipmentDto)
        {
            var entity = _context.VolunteerEquipments.Find(volunteerEquipmentDto.Id);
            if (entity != null)
            {
                entity.VolunteerId = volunteerEquipmentDto.VolunteerId;
                entity.EquipmentTypeId = volunteerEquipmentDto.EquipmentTypeId;

                _context.VolunteerEquipments.Update(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.VolunteerEquipments.Find(id);
            if (entity != null)
            {
                _context.VolunteerEquipments.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
