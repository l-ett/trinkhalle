using Microsoft.EntityFrameworkCore;
using Trinkhalle.Api.BeverageManagement.Domain;
using Trinkhalle.Api.BeverageManagement.Infrastructure;
using Trinkhalle.Api.CustomerManagement.Domain;

namespace Trinkhalle.Api;

public class TrinkhalleDbContext : DbContext
{
    public DbSet<Beverage> Beverages { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;

    public TrinkhalleDbContext(DbContextOptions<TrinkhalleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("Data");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BeverageEntityTypeConfiguration).Assembly);
    }
}