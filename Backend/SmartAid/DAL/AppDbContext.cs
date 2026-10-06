using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Volunteer> Volunteers { get; set; }
    public DbSet<EquipmentType> EquipmentTypes { get; set; }
    public DbSet<VolunteerEquipment> VolunteerEquipments { get; set; }
    public DbSet<HelpRequest> HelpRequests { get; set; }
    public DbSet<RequestEquipment> RequestEquipments { get; set; }
    public DbSet<RequestOffer> RequestOffers { get; set; }
}
