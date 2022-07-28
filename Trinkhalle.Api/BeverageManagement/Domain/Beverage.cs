using Trinkhalle.Api.Shared.Abstractions;

namespace Trinkhalle.Api.BeverageManagement.Domain;

public class Beverage : Aggregate
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public string ImageUrl { get; private set; }
    public bool Available { get; private set; }
    public int TotalPurchases { get; private set; }
    public DateTimeOffset LastPurchased { get; private set; }


    public Beverage(Guid id, decimal price, string name, string imageUrl, bool available)
    {
        if (Guid.Empty == id) throw new ArgumentException("Value cannot be empty.", nameof(id));
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
        Id = id;
        PartitionKey = id.ToString();
        Price = price;
        Name = name;
        ImageUrl = imageUrl;
        Available = available;
        TotalPurchases = 0;
    }

    public void Buy()
    {
        LastPurchased = DateTimeOffset.Now;
        TotalPurchases++;
    }
}