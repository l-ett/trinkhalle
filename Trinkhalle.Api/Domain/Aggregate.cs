using System;

namespace Trinkhalle.Api.Domain;

public abstract class Aggregate
{
    public Guid Id { get; protected init; }
    public string PartitionKey { get; protected init; } = null!;
}