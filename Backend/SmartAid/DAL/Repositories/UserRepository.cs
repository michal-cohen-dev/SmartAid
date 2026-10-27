
using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }

        public User? GetById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }

        public User? GetByEmail(string email)
        {
            return _context.Users
                .FirstOrDefault(u => u.Email == email);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }


        public void Update(User user)
        {
            var entity = _context.Users.Find(user.Id);

            if (entity != null)
            {
                entity.FullName = user.FullName;
                entity.Email = user.Email;
                entity.PhoneNumber = user.PhoneNumber;

                _context.SaveChanges();
            }
        }


        public void Delete(int id)
        {
            var entity = _context.Users.Find(id);

            if (entity != null)
            {
                _context.Users.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
