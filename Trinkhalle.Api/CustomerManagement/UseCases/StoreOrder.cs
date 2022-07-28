using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Trinkhalle.Api.BeverageManagement.UseCases;
using Trinkhalle.Api.CustomerManagement.Domain;

namespace Trinkhalle.Api.CustomerManagement.UseCases;

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
        [ServiceBusTrigger(topicName: nameof(BeveragePurchasedEvent),
            subscriptionName: FunctionName,
            Connection = "AzureServiceBus")]
        BeveragePurchasedEvent beveragePurchasedEvent)
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

    public sealed class PurchaseBeverageCommandValidator : AbstractValidator<StoreOrderCommand>
    {
        public PurchaseBeverageCommandValidator()
        {
            RuleFor(x => x.BeverageId).NotEmpty();
            RuleFor(x => x.BeverageName).NotEmpty();
            RuleFor(x => x.BeveragePrice).NotEmpty();
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    public class PurchaseBeverageCommandHandler : IRequestHandler<StoreOrderCommand, Result>
    {
        private readonly TrinkhalleDbContext _dbDbContext;

        public PurchaseBeverageCommandHandler(TrinkhalleDbContext dbContext)
        {
            _dbDbContext = dbContext;
        }

        public async Task<Result> Handle(StoreOrderCommand request, CancellationToken cancellationToken)
        {
            var order = new Order(request.OrderId, request.UserId, request.BeverageId, request.BeverageName,
                request.PurchasedAt, request.BeveragePrice);
            
            _dbDbContext.Orders.Add(order);

            await _dbDbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}