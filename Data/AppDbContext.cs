using BookEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BookEase.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Service>().HasData(
            new Service
            {
                Id = 1,
                Name = "Classic Haircut",
                Description = "A timeless cut tailored to your personal style. Includes consultation, wash, cut, and finish.",
                DurationMinutes = 30,
                Price = 25.00m
            },
            new Service
            {
                Id = 2,
                Name = "Beard Trim & Shape",
                Description = "Precision beard grooming and sculpting to keep your beard looking sharp and well-defined.",
                DurationMinutes = 30,
                Price = 18.00m
            },
            new Service
            {
                Id = 3,
                Name = "Haircut & Beard Combo",
                Description = "Our most popular service — a full haircut paired with a professional beard trim and shape.",
                DurationMinutes = 60,
                Price = 38.00m
            },
            new Service
            {
                Id = 4,
                Name = "Hot Towel Shave",
                Description = "Traditional straight-razor shave with warm towel prep, premium shave cream, and soothing aftercare.",
                DurationMinutes = 45,
                Price = 30.00m
            }
        );
    }
}
