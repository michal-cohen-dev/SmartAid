using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL.Repositories.Interfaces
{
    public interface IHelpRequestRepository
    {
        List<HelpRequestDto> GetAll();
        HelpRequestDto GetById(int id);
        void Add(HelpRequestDto helpRequestDto);
        void Update(HelpRequestDto helpRequestDto);
        void Delete(int id);
    }
}
