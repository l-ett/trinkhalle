using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Trinkhalle.DrinkManagement.Domain;
using Trinkhalle.DrinkManagement.Infrastructure;
using Trinkhalle.Shared.Events;

namespace Trinkhalle.DrinkManagement.Features;

public record StoreDrinkCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = null!;
    public bool Available { get; init; }
}

public class StoreDrink
{
    private const string FunctionName = $"{nameof(StoreDrink)}Function";

    private readonly IMediator _mediator;

    public StoreDrink(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task Run(
        [ServiceBusTrigger(topicName: nameof(DrinkCreatedEvent),
            subscriptionName: FunctionName,
            Connection = "AzureServiceBus")]
        DrinkCreatedEvent drinkCreatedEvent)
    {
        await _mediator.Send(
            new StoreDrinkCommand()
            {
                Id = drinkCreatedEvent.Id, Available = drinkCreatedEvent.Available,
                Name = drinkCreatedEvent.Name, ImageUrl = drinkCreatedEvent.ImageUrl,
                Price = drinkCreatedEvent.Price
            });
    }

    public sealed class StoreDrinkCommandValidator : AbstractValidator<StoreDrinkCommand>
    {
        public StoreDrinkCommandValidator()
        {
            RuleFor(x => x.Available).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.ImageUrl).NotNull();
        }
    }

    public class StoreDrinkCommandHandler : IRequestHandler<StoreDrinkCommand, Result>
    {
        private readonly DrinkManagementDbContext _dbContext;

        public StoreDrinkCommandHandler(DrinkManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result> Handle(StoreDrinkCommand request, CancellationToken cancellationToken)
        {
            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
            
            var beverage = new Drink(request.Id, request.Price, request.Name, request.ImageUrl, request.Available);

            var existing = await _dbContext.Drinks.FindAsync(new object?[] { beverage.Id },
                cancellationToken: cancellationToken);

            if (existing is not null) return Result.Ok();

            _dbContext.Drinks.Add(beverage);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}