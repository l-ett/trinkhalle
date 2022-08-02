using Azure.Messaging.ServiceBus;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Trinkhalle.CustomerManagement.Domain;
using Trinkhalle.CustomerManagement.Infrastructure;
using Trinkhalle.Shared.Events;
using Trinkhalle.Shared.Infrastructure;

namespace Trinkhalle.CustomerManagement.Features;

public class CreateInvoicesTrigger
{
    private const string FunctionName = $"CreateInvoicesFunction";

    private readonly IMediator _mediator;

    public CreateInvoicesTrigger(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task Run([TimerTrigger("0 */1 * * * *", UseMonitor = false)] TimerInfo timerInfo,
        FunctionContext context)
    {
        await _mediator.Send(new CreateInvoicesCommand());
    }
}

public record CreateInvoicesCommand : IRequest<Result>
{
}

public sealed class CreateInvoicesCommandValidator : AbstractValidator<CreateInvoicesCommand>
{
    public CreateInvoicesCommandValidator()
    {
    }
}

public class CreateInvoicesCommandHandler : IRequestHandler<CreateInvoicesCommand, Result>
{
    private readonly CustomerManagementDbContext _dbContext;
    private readonly IServicebusEventSender<InvoiceCreatedEvent> _eventSender;

    public CreateInvoicesCommandHandler(CustomerManagementDbContext dbContext,
        IServicebusEventSender<InvoiceCreatedEvent> eventSender)
    {
        _dbContext = dbContext;
        _eventSender = eventSender;
    }

    public async Task<Result> Handle(CreateInvoicesCommand request, CancellationToken cancellationToken)
    {
        var openOrders = await _dbContext.Orders.Where(order => order.Status == OrderStatus.Open)
            .ToListAsync(cancellationToken: cancellationToken);
        var openOrdersByUserId = openOrders.GroupBy(order => order.UserId);

        foreach (var value in openOrdersByUserId)
        {
            var orders = await FilterOutOrdersThatHaveAnInvoice(value);

            if (!orders.Any()) return Result.Ok();

            var invoiceCreatedEvent = new InvoiceCreatedEvent()
                { Id = Guid.NewGuid(), Orders = orders, UserId = value.Key };

            await _eventSender.Sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(invoiceCreatedEvent)),
                cancellationToken);
        }

        return Result.Ok();
    }

    private async Task<List<OrderDto>> FilterOutOrdersThatHaveAnInvoice(IGrouping<Guid, Order> openUserOrders)
    {
        var orders = new List<OrderDto>();

        var userInvoices =
            await _dbContext.Invoices.Where(invoice => invoice.UserId == openUserOrders.Key)
                .ToListAsync();

        foreach (var order in openUserOrders)
        {
            if (userInvoices.Any(i => i.OrderIds.Contains(order.Id.ToString()))) continue;

            orders.Add(new OrderDto()
            {
                Id = order.Id, BeverageId = order.BeverageId, BeverageName = order.BeverageName,
                Price = order.Price
            });
        }

        return orders;
    }
}