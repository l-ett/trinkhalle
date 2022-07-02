using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Trinkhalle.Api.Domain;
using Trinkhalle.Api.Infrastructure;

namespace Trinkhalle.Api.BeverageManagement;

public class StoreBeverage
{
    private const string FunctionName = $"{nameof(StoreBeverage)}Function";

    public record StoreBeverageCommand : IRequest<Result>
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public decimal Price { get; init; }
        public string ImageUrl { get; init; } = null!;
        public bool Available { get; init; }
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

    private readonly IMediator _mediator;

    public StoreBeverage(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task Run(
        [ServiceBusTrigger(topicName: nameof(CreateBeverage.BeverageCreatedEvent),
            subscriptionName: FunctionName,
            Connection = "AzureServiceBus")]
        CreateBeverage.BeverageCreatedEvent beverageCreatedEvent)
    {
        await _mediator.Send(
            new StoreBeverageCommand()
            {
                Id = beverageCreatedEvent.Id, Available = beverageCreatedEvent.Available,
                Name = beverageCreatedEvent.Name, ImageUrl = beverageCreatedEvent.ImageUrl,
                Price = beverageCreatedEvent.Price
            });
    }

    public class StoreBeverageCommandHandler : IRequestHandler<StoreBeverageCommand, Result>
    {
        private readonly TrinkhalleContext _dbContext;

        public StoreBeverageCommandHandler(TrinkhalleContext context)
        {
            _dbContext = context;
        }

        public async Task<Result> Handle(StoreBeverageCommand request, CancellationToken cancellationToken)
        {
            var beverage = new Beverage(request.Id, request.Price, request.Name, request.ImageUrl, request.Available);

            var existing = await _dbContext.Beverages.FindAsync(beverage.Id);

            if (existing is not null) return Result.Ok();

            _dbContext.Beverages.Add(beverage);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}