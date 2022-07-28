using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trinkhalle.Api.BeverageManagement.Domain;

namespace Trinkhalle.Api.BeverageManagement.Infrastructure;

public class BeverageEntityTypeConfiguration : IEntityTypeConfiguration<Beverage>
{
    public void Configure(EntityTypeBuilder<Beverage> builder)
    {
        builder
            .HasPartitionKey(b => b.PartitionKey)
            .UseETagConcurrency()
            .HasKey(b => new { b.Id });
    }
}