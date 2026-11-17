
using DTO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IVolunteerService
    {
        List<VolunteerDto> GetAll();

        VolunteerDto? GetById(int id);

        VolunteerDto? GetByUserId(int userId);

        void Add(RegisterVolunteerDto volunteerDto, int currentUserId);

        Task UpdateForAdmin(
            int id,
            AdminVolunteerUpdateDto volunteerDto,
            CancellationToken cancellationToken = default);

        Task Update(
            int id,
            VolunteerDto volunteerDto,
            int currentUserId,
            CancellationToken cancellationToken = default);

        void Delete(int id, int currentUserId);

        void DecreaseReliabilityScore(int volunteerId);

        void IncreaseReliabilityScore(int volunteerId);

        Task UpdateLocation(
            int userId,
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default);

        Task UpdateAvailabilityAsync(
            int userId,
            bool isAvailable,
            CancellationToken cancellationToken = default);
    }
}
