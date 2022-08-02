using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Trinkhalle.CustomerManagement.Domain;
using Trinkhalle.CustomerManagement.Infrastructure;
using Trinkhalle.Shared.Events;

namespace Trinkhalle.CustomerManagement.Features;

public record StoreOrderCommand : IRequest<Result>
{
    public Guid OrderId { get; set; }
    public Guid BeverageId { get; set; }
    public string BeverageName { get; set; } = null!;
    public decimal BeveragePrice { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset PurchasedAt { get; set; }
}

public class StoreOrder
{
    private const string FunctionName = $"{nameof(StoreOrder)}Function";

    private readonly IMediator _mediator;

    public StoreOrder(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task Run(
        [ServiceBusTrigger(topicName: nameof(DrinkPurchasedEvent),
            subscriptionName: FunctionName,
            Connection = "AzureServiceBus")]
        DrinkPurchasedEvent beveragePurchasedEvent)
    {
        await _mediator.Send(
            new StoreOrderCommand()
            {
                BeverageId = beveragePurchasedEvent.BeverageId,
                BeverageName = beveragePurchasedEvent.BeverageName,
                BeveragePrice = beveragePurchasedEvent.BeveragePrice,
                OrderId = beveragePurchasedEvent.Id,
                UserId = beveragePurchasedEvent.UserId,
                PurchasedAt = beveragePurchasedEvent.PurchasedAt
            });
    }

    public sealed class StoreOrderCommandValidator : AbstractValidator<StoreOrderCommand>
    {
        public StoreOrderCommandValidator()
        {
            RuleFor(x => x.BeverageId).NotEmpty();
            RuleFor(x => x.BeverageName).NotEmpty();
            RuleFor(x => x.BeveragePrice).NotEmpty();
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    public class StoreOrderCommandHandler : IRequestHandler<StoreOrderCommand, Result>
    {
        private readonly CustomerManagementDbContext _dbContext;

        public StoreOrderCommandHandler(CustomerManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result> Handle(StoreOrderCommand request, CancellationToken cancellationToken)
        {
            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

            var existingOrder = await _dbContext.Orders.FindAsync(new object?[] { request.OrderId },
                cancellationToken: cancellationToken);

            if (existingOrder is not null) return Result.Ok();

            var order = new Order(request.OrderId, request.UserId, request.BeverageId, request.BeverageName,
                request.PurchasedAt, request.BeveragePrice);

            _dbContext.Orders.Add(order);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}