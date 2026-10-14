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
    public class HelpRequestRepository : IHelpRequestRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public HelpRequestRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // שימוש ב-ProjectTo להצגה מהירה של כל בקשות העזרה
        public List<HelpRequestDto> GetAll()
        {
            return _context.HelpRequests
                .Include(hr => hr.User)
                .Include(hr => hr.AssignedVolunteer)
                .ProjectTo<HelpRequestDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        // שימוש ב-ProjectTo להצגה מהירה לפי מזהה
        public HelpRequestDto GetById(int id)
        {
            return _context.HelpRequests
                .Where(hr => hr.Id == id)
                .Include(hr => hr.User)
                .Include(hr => hr.AssignedVolunteer)
                .ProjectTo<HelpRequestDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault()!;
        }

        public void Add(HelpRequestDto helpRequestDto)
        {
            var entity = _mapper.Map<HelpRequest>(helpRequestDto);
            entity.CreatedAt = DateTime.Now;
            entity.Status = (RequestStatus)1;

            _context.HelpRequests.Add(entity);
            _context.SaveChanges(); // שומר סינכרונית במסד הנתונים
        }

        public void Update(HelpRequestDto helpRequestDto)
        {
            var entity = _context.HelpRequests.Find(helpRequestDto.Id);
            if (entity != null)
            {
                entity.Status = (RequestStatus)helpRequestDto.Status;
                entity.AssignedVolunteerId = helpRequestDto.AssignedVolunteerId;
                entity.Rating = helpRequestDto.Rating;
                // עדכון שדות נוספים בהתאם לצורך

                _context.HelpRequests.Update(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.HelpRequests.Find(id);
            if (entity != null)
            {
                _context.HelpRequests.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
