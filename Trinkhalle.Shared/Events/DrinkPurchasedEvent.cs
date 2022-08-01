namespace Trinkhalle.Shared.Events;

public record DrinkPurchasedEvent
{
    public Guid Id { get; set; }
    public Guid BeverageId { get; set; }
    public string BeverageName { get; set; } = null!;
    public decimal BeveragePrice { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset PurchasedAt { get; set; }
}