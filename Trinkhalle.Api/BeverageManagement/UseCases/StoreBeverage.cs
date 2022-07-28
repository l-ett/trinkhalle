using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Trinkhalle.Api.BeverageManagement.Domain;

namespace Trinkhalle.Api.BeverageManagement.UseCases;

public record StoreBeverageCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = null!;
    public bool Available { get; init; }
}

public class StoreBeverage
{
    private const string FunctionName = $"{nameof(StoreBeverage)}Function";

    private readonly IMediator _mediator;

    public StoreBeverage(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task Run(
        [ServiceBusTrigger(topicName: nameof(BeverageCreatedEvent),
            subscriptionName: FunctionName,
            Connection = "AzureServiceBus")]
        BeverageCreatedEvent beverageCreatedEvent)
    {
        await _mediator.Send(
            new StoreBeverageCommand()
            {
                Id = beverageCreatedEvent.Id, Available = beverageCreatedEvent.Available,
                Name = beverageCreatedEvent.Name, ImageUrl = beverageCreatedEvent.ImageUrl,
                Price = beverageCreatedEvent.Price
            });
    }

    public sealed class StoreBeverageCommandValidator : AbstractValidator<StoreBeverageCommand>
    {
        public StoreBeverageCommandValidator()
        {
            RuleFor(x => x.Available).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.ImageUrl).NotNull();
        }
    }

    public class StoreBeverageCommandHandler : IRequestHandler<StoreBeverageCommand, Result>
    {
        private readonly TrinkhalleDbContext _dbContext;

        public StoreBeverageCommandHandler(TrinkhalleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result> Handle(StoreBeverageCommand request, CancellationToken cancellationToken)
        {
            var beverage = new Beverage(request.Id, request.Price, request.Name, request.ImageUrl, request.Available);

            var existing = await _dbContext.Beverages.FindAsync(new object?[] { beverage.Id },
                cancellationToken: cancellationToken);

            if (existing is not null) return Result.Ok();

            _dbContext.Beverages.Add(beverage);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}