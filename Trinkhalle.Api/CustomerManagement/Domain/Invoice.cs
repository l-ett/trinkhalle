using Trinkhalle.Api.Shared.Abstractions;

namespace Trinkhalle.Api.CustomerManagement.Domain;

public enum InvoiceStatus
{
    Open,
    Payed
}

public class Invoice : Aggregate
{
    public Guid UserId { get; init; }
    public IList<string> OrderIds { get; init; }
    public IList<InvoiceElement> InvoiceElements { get; init; }
    public InvoiceStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }


    public Invoice(Guid id, Guid userId)
    {
        Id = id;
        UserId = userId;
        InvoiceElements = new List<InvoiceElement>();
        OrderIds = new List<string>();
        PartitionKey = userId.ToString();
        Status = InvoiceStatus.Open;
        CreatedAt = DateTimeOffset.Now;
    }

    public void AddInvoiceElements(IEnumerable<InvoiceElement> invoiceElements)
    {
        invoiceElements.ToList().ForEach(e =>
        {
            OrderIds.Add(e.OrderId.ToString());
            InvoiceElements.Add(e);
        });
    }

    public override string ToString()
    {
        return $"Id : {Id} \n " +
               $"User : {UserId} \n " +
               $"Status : {Status} \n " +
               $"CreatedAt : {CreatedAt} \n " +
               $"Total: {TotalPrice()}";
    }

    public decimal TotalPrice()
    {
        return InvoiceElements.Sum(ie => ie.Price);
    }
}