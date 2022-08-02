using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Trinkhalle.DrinkManagement.Infrastructure;
using Trinkhalle.Shared.Events;

namespace Trinkhalle.DrinkManagement.Features;

public record PurchaseDrinkCommand : IRequest<Result>
{
    public Guid BeverageId { get; set; }
}

public class PurchaseDrink
{
    private const string FunctionName = $"{nameof(PurchaseDrink)}Function";

    private readonly IMediator _mediator;

    public PurchaseDrink(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task Run(
        [ServiceBusTrigger(topicName: nameof(DrinkPurchasedEvent),
            subscriptionName: FunctionName,
            Connection = "AzureServiceBus")]
        DrinkPurchasedEvent drinkPurchasedEvent)
    {
        await _mediator.Send(
            new PurchaseDrinkCommand()
            {
                BeverageId = drinkPurchasedEvent.BeverageId
            });
    }

    public sealed class PurchaseBeverageCommandValidator : AbstractValidator<PurchaseDrinkCommand>
    {
        public PurchaseBeverageCommandValidator()
        {
            RuleFor(x => x.BeverageId).NotEmpty();
        }
    }

    public class PurchaseDrinkCommandHandler : IRequestHandler<PurchaseDrinkCommand, Result>
    {
        private readonly DrinkManagementDbContext _dbDbContext;

        public PurchaseDrinkCommandHandler(DrinkManagementDbContext dbContext)
        {
            _dbDbContext = dbContext;
        }

        public async Task<Result> Handle(PurchaseDrinkCommand request, CancellationToken cancellationToken)
        {
            var drink = await _dbDbContext.Drinks.FindAsync(new object?[] { request.BeverageId },
                cancellationToken: cancellationToken);

            if (drink is null) return Result.Fail("beverage not found");

            drink.Buy();

            _dbDbContext.Update(drink);

            await _dbDbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}