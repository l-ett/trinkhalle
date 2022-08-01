using Microsoft.EntityFrameworkCore;
using Trinkhalle.CustomerManagement.Domain;

namespace Trinkhalle.CustomerManagement.Infrastructure;

public class CustomerManagementDbContext : DbContext
{
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;

    public CustomerManagementDbContext(DbContextOptions<CustomerManagementDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("CustomerManagement");
        modelBuilder.Entity<Invoice>()
            .HasPartitionKey(b => b.PartitionKey)
            .UseETagConcurrency()
            .HasKey(b => new { b.Id });
        modelBuilder.Entity<Order>()
            .HasPartitionKey(b => b.PartitionKey)
            .UseETagConcurrency()
            .HasKey(b => new { b.Id });
    }
}