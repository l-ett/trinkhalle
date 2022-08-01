namespace Trinkhalle.Api.CustomerManagement.Domain;

public record InvoiceElement
{
    public Guid OrderId { get; init; }
    public Guid BeverageId { get; init; }
    public string BeverageName { get; init; } = null!;
    public decimal Price { get; init; }
}