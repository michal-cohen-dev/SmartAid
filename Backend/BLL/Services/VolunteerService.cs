
using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using BLL.Services;

namespace BLL.Services
{
    public class VolunteerService : IVolunteerService
    {
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IMapper _mapper;

        public VolunteerService(
            IVolunteerRepository volunteerRepository,
            IMapper mapper)
        {
            _volunteerRepository = volunteerRepository;
            _mapper = mapper;
        }

        public List<VolunteerDto> GetAll()
        {
            var volunteers = _volunteerRepository.GetAll();
            return _mapper.Map<List<VolunteerDto>>(volunteers);
        }

        public VolunteerDto? GetById(int id)
        {
            var volunteer = _volunteerRepository.GetById(id);

            if (volunteer == null)
                return null;

            return _mapper.Map<VolunteerDto>(volunteer);
        }

        public void Add(VolunteerDto volunteerDto)
        {
            var volunteer = _mapper.Map<Volunteer>(volunteerDto);

            // Business defaults
            volunteer.IsActive = true;
            volunteer.ReliabilityScore = 100.0;

            _volunteerRepository.Add(volunteer);
        }

        public void Update(int id, VolunteerDto volunteerDto)
        {
            var existingVolunteer = _volunteerRepository.GetById(id);

            if (existingVolunteer == null)
                return;

            var volunteer = _mapper.Map<Volunteer>(volunteerDto);
            volunteer.Id = id;

            _volunteerRepository.Update(volunteer);
        }

        public void Delete(int id)
        {
            _volunteerRepository.Delete(id);
        }
    }
}

