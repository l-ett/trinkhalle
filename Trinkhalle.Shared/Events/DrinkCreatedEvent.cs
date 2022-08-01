namespace Trinkhalle.Shared.Events;

public record DrinkCreatedEvent
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = null!;
    public bool Available { get; init; }
}