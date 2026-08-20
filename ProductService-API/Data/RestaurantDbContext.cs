using Microsoft.EntityFrameworkCore;
using RestaurantService.API.Models;

namespace RestaurantService.API.Data;

public class RestaurantDbContext : DbContext
{
    public RestaurantDbContext(
        DbContextOptions<RestaurantDbContext> options)
        : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();

    public DbSet<MenuCategory> MenuCategories =>
        Set<MenuCategory>();

    public DbSet<MenuItem> MenuItems =>
        Set<MenuItem>();

    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Restaurant>()
            .Property(x => x.Rating)
            .HasPrecision(3, 2);

        modelBuilder.Entity<MenuItem>()
            .Property(x => x.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Restaurant>()
            .HasIndex(x => new
            {
                x.Latitude,
                x.Longitude
            });

        modelBuilder.Entity<Restaurant>()
            .HasIndex(x => new
            {
                x.IsActive,
                x.IsOpen
            });

        modelBuilder.Entity<MenuItem>()
            .HasIndex(x => new
            {
                x.RestaurantId,
                x.IsAvailable
            });

        modelBuilder.Entity<MenuCategory>()
            .HasOne(x => x.Restaurant)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuItem>()
            .HasOne(x => x.Restaurant)
            .WithMany()
            .HasForeignKey(x => x.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MenuItem>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}