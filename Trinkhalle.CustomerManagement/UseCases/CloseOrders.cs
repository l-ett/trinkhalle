using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Trinkhalle.CustomerManagement.Infrastructure;
using Trinkhalle.Shared.Events;

namespace Trinkhalle.CustomerManagement.UseCases;

public record CloseOrdersCommand : IRequest<Result>
{
    public IEnumerable<Guid> OrderIds { get; set; } = null!;
}

public class CloseOrders
{
    private const string FunctionName = $"{nameof(CloseOrders)}Function";

    private readonly IMediator _mediator;

    public CloseOrders(IMediator mediator)
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
            new CloseOrdersCommand()
            {
                OrderIds = invoiceCreatedEvent.Orders.Select(o => o.Id)
            });
    }

    public sealed class CloseOrdersCommandValidator : AbstractValidator<CloseOrdersCommand>
    {
        public CloseOrdersCommandValidator()
        {
        }
    }

    public class CloseOrdersCommandHandler : IRequestHandler<CloseOrdersCommand, Result>
    {
        private readonly CustomerManagementDbContext _dbContext;

        public CloseOrdersCommandHandler(CustomerManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result> Handle(CloseOrdersCommand request, CancellationToken cancellationToken)
        {
            var orders = await _dbContext.Orders.Where(order => request.OrderIds.Contains(order.Id))
                .ToListAsync(cancellationToken);

            orders.ForEach(order => order.CloseOrder());

            _dbContext.Orders.UpdateRange(orders);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}