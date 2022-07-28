using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;

namespace Trinkhalle.Api.BeverageManagement.UseCases;

public record PurchaseBeverageCommand : IRequest<Result>
{
    public Guid BeverageId { get; set; }
}

public class PurchaseBeverage
{
    private const string FunctionName = $"{nameof(PurchaseBeverage)}Function";

    private readonly IMediator _mediator;

    public PurchaseBeverage(IMediator mediator)
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
            new PurchaseBeverageCommand()
            {
                BeverageId = beveragePurchasedEvent.BeverageId
            });
    }

    public sealed class PurchaseBeverageCommandValidator : AbstractValidator<PurchaseBeverageCommand>
    {
        public PurchaseBeverageCommandValidator()
        {
            RuleFor(x => x.BeverageId).NotEmpty();
        }
    }

    public class PurchaseBeverageCommandHandler : IRequestHandler<PurchaseBeverageCommand, Result>
    {
        private readonly TrinkhalleDbContext _dbDbContext;

        public PurchaseBeverageCommandHandler(TrinkhalleDbContext dbContext)
        {
            _dbDbContext = dbContext;
        }

        public async Task<Result> Handle(PurchaseBeverageCommand request, CancellationToken cancellationToken)
        {
            var beverage = await _dbDbContext.Beverages.FindAsync(new object?[] { request.BeverageId },
                cancellationToken: cancellationToken);

            if (beverage is null) return Result.Fail("beverage not found");

            beverage.Buy();

            _dbDbContext.Update(beverage);

            await _dbDbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}