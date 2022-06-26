using Microsoft.EntityFrameworkCore;
using Trinkhalle.Api.Domain;

namespace Trinkhalle.Api.Infrastructure;

public class TrinkhalleContext : DbContext
{
    public DbSet<Beverage> Beverages { get; set; } = null!;

    public TrinkhalleContext(DbContextOptions<TrinkhalleContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("Data");

        modelBuilder.Entity<Beverage>()
            .HasPartitionKey(b => b.PartitionKey)
            .UseETagConcurrency()
            .HasKey(b => new { b.Id });
    }
}