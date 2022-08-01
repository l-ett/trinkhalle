namespace Trinkhalle.Shared.Events;

public record InvoiceCreatedEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public IEnumerable<OrderDto> Orders { get; set; } = null!;
}

public record OrderDto
{
    public Guid Id { get; set; }
    public Guid BeverageId { get; set; }
    public string BeverageName { get; set; } = null!;
    public decimal Price { get; set; }
}