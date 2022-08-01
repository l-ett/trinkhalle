namespace Trinkhalle.Shared.Abstractions;

public abstract class Aggregate
{
    public Guid Id { get; protected init; }
    public string PartitionKey { get; protected init; } = null!;
}