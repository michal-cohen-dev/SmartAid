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
    public class VolunteerRepository : IVolunteerRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public VolunteerRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // שימוש ב-ProjectTo להצגה מהירה של כל המתנדבים
        public List<VolunteerDto> GetAll()
        {
            return _context.Volunteers
                .ProjectTo<VolunteerDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        // שימוש ב-ProjectTo להצגה מהירה לפי מזהה
        public VolunteerDto GetById(int id)
        {
            return _context.Volunteers
                .Where(v => v.Id == id)
                .ProjectTo<VolunteerDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault()!;
        }

        public void Add(VolunteerDto volunteerDto)
        {
            var entity = _mapper.Map<Volunteer>(volunteerDto);
            entity.IsActive = true; // מתנדב חדש מתחיל כפעיל כברירת מחדל
            entity.ReliabilityScore = 100.0; // ציון אמינות התחלתי

            _context.Volunteers.Add(entity);
            _context.SaveChanges(); // שומר סינכרונית במסד הנתונים
        }

        public void Update(VolunteerDto volunteerDto)
        {
            var entity = _context.Volunteers.Find(volunteerDto.Id);
            if (entity != null)
            {
                entity.FullName = volunteerDto.FullName;
                entity.PhoneNumber = volunteerDto.PhoneNumber;
                entity.IsActive = volunteerDto.IsActive;
                entity.TotalOffersReceived = volunteerDto.TotalOffersReceived;
                entity.TotalOffersAccepted = volunteerDto.TotalOffersAccepted;
                entity.TotalCompleted = volunteerDto.TotalCompleted;
                entity.ReliabilityScore = volunteerDto.ReliabilityScore;

                _context.Volunteers.Update(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.Volunteers.Find(id);
            if (entity != null)
            {
                _context.Volunteers.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
