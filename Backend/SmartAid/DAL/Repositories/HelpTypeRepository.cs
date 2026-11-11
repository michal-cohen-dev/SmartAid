using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class HelpTypeRepository : IHelpTypeRepository
    {
        private readonly AppDbContext _context;

        public HelpTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<HelpType> GetAll()
        {
            return _context.HelpTypes.ToList();
        }

        public HelpType? GetById(int id)
        {
            return _context.HelpTypes
                .FirstOrDefault(h => h.Id == id);
        }

        public void Add(HelpType helpType)
        {
            _context.HelpTypes.Add(helpType);
            _context.SaveChanges();
        }

        public void Update(HelpType helpType)
        {
            var entity = _context.HelpTypes.Find(helpType.Id);

            if (entity != null)
            {
                entity.Name = helpType.Name;

                _context.HelpTypes.Update(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.HelpTypes.Find(id);

            if (entity != null)
            {
                _context.HelpTypes.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
