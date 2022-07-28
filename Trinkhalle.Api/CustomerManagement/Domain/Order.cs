using Trinkhalle.Api.Shared.Abstractions;

namespace Trinkhalle.Api.CustomerManagement.Domain;

public class Order : Aggregate
{
    public Order(Guid id, Guid userId, Guid beverageId, string beverageName, DateTimeOffset purchasedAt, decimal price)
    {
        Id = id;
        UserId = userId;
        BeverageId = beverageId;
        BeverageName = beverageName;
        PurchasedAt = purchasedAt;
        Price = price;
        PartitionKey = userId.ToString();
    }

    public Guid UserId { get; init; }
    public Guid BeverageId { get; init; }
    public string BeverageName { get; init; }
    public DateTimeOffset PurchasedAt { get; init; }
    public decimal Price { get; init; }
}