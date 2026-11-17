using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using BLL.Services.Interfaces;
using DAL.Repositories.Interfaces;
using DTO;

namespace BLL.Services
{
    public class HelpTypeService : IHelpTypeService
    {
        private readonly IHelpTypeRepository _repository;
        private readonly IMapper _mapper;

        public HelpTypeService(
            IHelpTypeRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public List<HelpTypeDto> GetAll()
        {
            var helpTypes = _repository.GetAll();

            return _mapper.Map<List<HelpTypeDto>>(helpTypes);
        }
    }
}
