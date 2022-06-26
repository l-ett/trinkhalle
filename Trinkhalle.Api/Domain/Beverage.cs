using System;

namespace Trinkhalle.Api.Domain;

public class Beverage : Aggregate
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public string ImageUrl { get; private set; }
    public bool Available { get; private set; }

    public Beverage(Guid id, decimal price, string name, string imageUrl, bool available)
    {
        Id = id;
        PartitionKey = id.ToString();
        Price = price;
        Name = name;
        ImageUrl = imageUrl;
        Available = available;
    }
}