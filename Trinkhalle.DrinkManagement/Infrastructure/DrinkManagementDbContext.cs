using Microsoft.EntityFrameworkCore;
using Trinkhalle.DrinkManagement.Domain;

namespace Trinkhalle.DrinkManagement.Infrastructure;

public class DrinkManagementDbContext : DbContext
{
    public DbSet<Drink> Drinks { get; set; } = null!;

    public DrinkManagementDbContext(DbContextOptions<DrinkManagementDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("DrinkManagement");
        modelBuilder.Entity<Drink>()
            .HasPartitionKey(b => b.PartitionKey)
            .UseETagConcurrency()
            .HasKey(b => new { b.Id });
    }
}