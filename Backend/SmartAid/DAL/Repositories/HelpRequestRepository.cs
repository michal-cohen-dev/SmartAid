using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{


    public class HelpRequestRepository : IHelpRequestRepository
    {
        private readonly AppDbContext _context;

        public HelpRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<HelpRequest> GetAll()
        {
            return _context.HelpRequests
                .Include(hr => hr.User)
                .Include(hr => hr.AssignedVolunteer)
                .ToList();
        }

        public HelpRequest? GetById(int id)
        {
            return _context.HelpRequests
                .Include(hr => hr.User)
                .Include(hr => hr.AssignedVolunteer)
                .FirstOrDefault(hr => hr.Id == id);
        }

        public HelpRequest? GetOwned(int id, int userId)
        {
            return _context.HelpRequests
                .Include(hr => hr.User)
                .Include(hr => hr.AssignedVolunteer)
                .FirstOrDefault(hr =>
                    hr.Id == id && hr.UserId == userId);
        }

        public void Add(HelpRequest entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            _context.HelpRequests.Add(entity);
            _context.SaveChanges();
        }

        public void Update(HelpRequest entity)
        {
            _context.HelpRequests.Update(entity);
            _context.SaveChanges();
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