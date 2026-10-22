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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- User ----------
        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.Property(u => u.PhoneNumber).HasMaxLength(20);
            e.HasIndex(u => u.Email).IsUnique();
        });

        // ---------- Volunteer ----------
        modelBuilder.Entity<Volunteer>(e =>
        {
            e.Property(v => v.FullName).IsRequired().HasMaxLength(100);
            e.Property(v => v.PhoneNumber).HasMaxLength(20);
        });

        // ---------- EquipmentType ----------
        modelBuilder.Entity<EquipmentType>(e =>
        {
            e.Property(t => t.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(t => t.Name).IsUnique();
        });

        // ---------- VolunteerEquipment ----------
        modelBuilder.Entity<VolunteerEquipment>(e =>
        {
            e.HasOne(ve => ve.Volunteer)
             .WithMany(v => v.VolunteerEquipments)
             .HasForeignKey(ve => ve.VolunteerId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ve => ve.EquipmentType)
             .WithMany(t => t.VolunteerEquipments)
             .HasForeignKey(ve => ve.EquipmentTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            // a volunteer can't have the same equipment twice
            e.HasIndex(ve => new { ve.VolunteerId, ve.EquipmentTypeId }).IsUnique();
        });

        // ---------- HelpRequest ----------
        modelBuilder.Entity<HelpRequest>(e =>
        {
            e.Property(h => h.Address).HasMaxLength(300);
            e.Property(h => h.AddressNotes).HasMaxLength(300);

            // stored as text ("Open", "Assigned"...) so the atomic UPDATE ... WHERE Status='Open' is readable
            e.Property(h => h.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(h => h.User)
             .WithMany(u => u.HelpRequests)
             .HasForeignKey(h => h.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(h => h.AssignedVolunteer)
             .WithMany(v => v.AssignedRequests)
             .HasForeignKey(h => h.AssignedVolunteerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(h => h.Status);
        });

        // ---------- RequestEquipment ----------
        modelBuilder.Entity<RequestEquipment>(e =>
        {
            e.HasOne(re => re.HelpRequest)
             .WithMany(h => h.RequiredEquipments)
             .HasForeignKey(re => re.HelpRequestId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(re => re.EquipmentType)
             .WithMany(t => t.RequestEquipments)
             .HasForeignKey(re => re.EquipmentTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(re => new { re.HelpRequestId, re.EquipmentTypeId }).IsUnique();
        });

        // ---------- RequestOffer ----------
        modelBuilder.Entity<RequestOffer>(e =>
        {
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(o => o.HelpRequest)
             .WithMany(h => h.Offers)
             .HasForeignKey(o => o.RequestId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(o => o.Volunteer)
             .WithMany(v => v.RequestOffers)
             .HasForeignKey(o => o.VolunteerId)
             .OnDelete(DeleteBehavior.Restrict);

            // the same request is never offered twice to the same volunteer
            e.HasIndex(o => new { o.RequestId, o.VolunteerId }).IsUnique();
        });
    }
}