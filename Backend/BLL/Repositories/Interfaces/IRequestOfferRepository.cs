using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL.Repositories.Interfaces
{
    public interface IRequestOfferRepository
    {
        List<RequestOfferDto> GetAll();
        RequestOfferDto GetById(int id);
        void Add(RequestOfferDto requestOfferDto);
        void Update(RequestOfferDto requestOfferDto);
        void Delete(int id);
    }
}
