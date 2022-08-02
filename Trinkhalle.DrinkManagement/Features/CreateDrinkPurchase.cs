using System.Net;
using Azure.Messaging.ServiceBus;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Trinkhalle.DrinkManagement.Infrastructure;
using Trinkhalle.Shared.Events;
using Trinkhalle.Shared.Extensions;
using Trinkhalle.Shared.Infrastructure;

namespace Trinkhalle.DrinkManagement.Features;

public record CreateDrinkPurchaseCommand : IRequest<Result<Guid>>
{
    public Guid BeverageId { get; set; }
    public Guid UserId { get; set; }
}

public class CreateDrinkPurchase
{
    private const string FunctionName = $"{nameof(CreateDrinkPurchase)}Function";

    private readonly IMediator _mediator;

    public CreateDrinkPurchase(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DrinkManagement/PurchaseDrink")]
        HttpRequestData requestData)
    {
        var command = await requestData.ReadFromJsonAsync<CreateDrinkPurchaseCommand>();

        if (command is null) return requestData.CreateResponse(HttpStatusCode.BadRequest);

        var result = await _mediator.Send(command);

        if (result.IsFailed) return await requestData.CreateResponseAsync(HttpStatusCode.BadRequest, result);

        return await requestData.CreateResponseAsync(HttpStatusCode.OK, result);
    }

    public sealed class CreateBeveragePurchaseCommandValidator : AbstractValidator<CreateDrinkPurchaseCommand>
    {
        public CreateBeveragePurchaseCommandValidator()
        {
            RuleFor(x => x.BeverageId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    public class CreateBeveragePurchaseCommandHandler : IRequestHandler<CreateDrinkPurchaseCommand, Result<Guid>>
    {
        private readonly IServicebusEventSender<DrinkPurchasedEvent> _eventSender;
        private readonly DrinkManagementDbContext _dbContext;

        public CreateBeveragePurchaseCommandHandler(DrinkManagementDbContext dbContext,
            IServicebusEventSender<DrinkPurchasedEvent> eventSender)
        {
            _eventSender = eventSender;
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> Handle(CreateDrinkPurchaseCommand request,
            CancellationToken cancellationToken)
        {
            var existing = await _dbContext.Drinks.FindAsync(new object?[] { request.BeverageId },
                cancellationToken: cancellationToken);

            if (existing is null) return Result.Fail("Beverage not found");

            var purchasedEvent = new DrinkPurchasedEvent()
            {
                Id = Guid.NewGuid(),
                BeverageId = request.BeverageId,
                BeverageName = existing.Name,
                BeveragePrice = existing.Price,
                UserId = request.UserId,
                PurchasedAt = DateTimeOffset.Now
            };

            await _eventSender.Sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(purchasedEvent)),
                cancellationToken);

            return Result.Ok(purchasedEvent.Id);
        }
    }
}