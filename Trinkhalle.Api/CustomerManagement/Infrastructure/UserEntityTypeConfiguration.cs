using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trinkhalle.Api.CustomerManagement.Domain;

namespace Trinkhalle.Api.CustomerManagement.Infrastructure;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasPartitionKey(b => b.PartitionKey)
            .UseETagConcurrency()
            .HasKey(b => new { b.Id });
    }
}