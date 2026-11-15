using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using System.Threading;
using System.Threading.Tasks;
using BLL.Services.Models;

namespace BLL.Services.Interfaces
{
    public interface IVolunteerMatchingService
    {
        Task<List<VolunteerMatchResult>> GetEligibleVolunteersAsync(
      int helpRequestId,
      CancellationToken cancellationToken = default);
    }
}