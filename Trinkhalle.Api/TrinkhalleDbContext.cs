using Microsoft.EntityFrameworkCore;
using Trinkhalle.Api.BeverageManagement.Domain;
using Trinkhalle.Api.BeverageManagement.Infrastructure;
using Trinkhalle.Api.CustomerManagement.Domain;
using Trinkhalle.Api.CustomerManagement.Infrastructure;

namespace Trinkhalle.Api;

public class TrinkhalleDbContext : DbContext
{
    public DbSet<Beverage> Beverages { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;

    public TrinkhalleDbContext(DbContextOptions<TrinkhalleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("Data");
        new BeverageEntityTypeConfiguration().Configure(modelBuilder.Entity<Beverage>());
        new InvoiceEntityTypeConfiguration().Configure(modelBuilder.Entity<Invoice>());
        new OrderEntityTypeConfiguration().Configure(modelBuilder.Entity<Order>());
    }
}