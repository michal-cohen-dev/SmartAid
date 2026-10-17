using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{


    public class RequestEquipmentRepository : IRequestEquipmentRepository
    {
        private readonly AppDbContext _context;

        public RequestEquipmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<RequestEquipment> GetAll()
        {
            return _context.RequestEquipments
                .Include(re => re.HelpRequest)
                .Include(re => re.EquipmentType)
                .ToList();
        }

        public RequestEquipment? GetById(int id)
        {
            return _context.RequestEquipments
                .Include(re => re.HelpRequest)
                .Include(re => re.EquipmentType)
                .FirstOrDefault(re => re.Id == id);
        }

        public void Add(RequestEquipment requestEquipment)
        {
            _context.RequestEquipments.Add(requestEquipment);
            _context.SaveChanges();
        }

        public void Update(RequestEquipment requestEquipment)
        {
            _context.RequestEquipments.Update(requestEquipment);
            _context.SaveChanges();
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