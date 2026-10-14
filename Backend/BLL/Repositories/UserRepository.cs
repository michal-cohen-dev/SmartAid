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
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // שימוש ב-ProjectTo להצגה מהירה (Read-only query)
        public List<UserDto> GetAll()
        {
            return _context.Users
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        // שימוש ב-ProjectTo להצגה מהירה לפי מזהה
        public UserDto GetById(int id)
        {
            return _context.Users
                .Where(u => u.Id == id)
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault()!;
        }

        public void Add(UserDto userDto)
        {
            var entity = _mapper.Map<User>(userDto);
            entity.CreatedAt = DateTime.Now;
            _context.Users.Add(entity);
            _context.SaveChanges(); // שומר סינכרונית במסד הנתונים
        }

        // שימוש בטעינה ועדכון (כולל מעקב שינויים)
        public void Update(UserDto userDto)
        {
            var entity = _context.Users.Find(userDto.Id);
            if (entity != null)
            {
                _mapper.Map(userDto, entity);
                _context.Users.Update(entity);
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
