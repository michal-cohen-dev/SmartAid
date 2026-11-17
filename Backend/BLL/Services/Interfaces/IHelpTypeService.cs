
using DTO;
using System.Collections.Generic;

namespace BLL.Services.Interfaces
{
    public interface IHelpTypeService
    {
        List<HelpTypeDto> GetAll();
        HelpTypeDto? GetById(int id);
        void Add(HelpTypeDto helpTypeDto);
        void Update(int id, HelpTypeDto helpTypeDto);
        void Delete(int id);
    }
}
