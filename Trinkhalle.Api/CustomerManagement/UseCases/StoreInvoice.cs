using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Trinkhalle.Api.CustomerManagement.Domain;

namespace Trinkhalle.Api.CustomerManagement.UseCases;

public record StoreInvoiceCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public IEnumerable<OrderDto> Orders { get; set; } = null!;
}

public class StoreInvoice
{
    private const string FunctionName = $"{nameof(StoreInvoice)}Function";

    private readonly IMediator _mediator;

    public StoreInvoice(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task Run(
        [ServiceBusTrigger(topicName: nameof(InvoiceCreatedEvent),
            subscriptionName: FunctionName,
            Connection = "AzureServiceBus")]
        InvoiceCreatedEvent invoiceCreatedEvent)
    {
        await _mediator.Send(
            new StoreInvoiceCommand()
            {
                Id = invoiceCreatedEvent.Id,
                Orders = invoiceCreatedEvent.Orders,
                UserId = invoiceCreatedEvent.UserId
            });
    }

    public sealed class StoreInvoiceCommandValidator : AbstractValidator<StoreInvoiceCommand>
    {
        public StoreInvoiceCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    public class StoreInvoiceCommandHandler : IRequestHandler<StoreInvoiceCommand, Result>
    {
        private readonly TrinkhalleDbContext _dbContext;

        public StoreInvoiceCommandHandler(TrinkhalleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result> Handle(StoreInvoiceCommand request, CancellationToken cancellationToken)
        {
            var invoice = new Invoice(request.Id, request.UserId);

            var existingInvoice = await _dbContext.Invoices.FindAsync(new object?[] { invoice.Id },
                cancellationToken: cancellationToken);

            if (existingInvoice is not null) return Result.Ok();

            var invoiceElements = request.Orders.Select(order =>
                new InvoiceElement()
                {
                    Price = order.Price,
                    BeverageId = order.BeverageId,
                    BeverageName = order.BeverageName,
                    OrderId = order.Id
                });
            invoice.AddInvoiceElements(invoiceElements);

            _dbContext.Invoices.Add(invoice);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}