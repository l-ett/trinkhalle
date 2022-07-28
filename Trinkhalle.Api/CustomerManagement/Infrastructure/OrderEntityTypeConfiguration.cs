using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trinkhalle.Api.CustomerManagement.Domain;

namespace Trinkhalle.Api.CustomerManagement.Infrastructure;

public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder
            .HasPartitionKey(b => b.PartitionKey)
            .UseETagConcurrency()
            .HasKey(b => new { b.Id });
    }
}