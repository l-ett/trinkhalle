using System.Net;
using Azure.Messaging.ServiceBus;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Trinkhalle.Api.Shared.Extensions;
using Trinkhalle.Api.Shared.Infrastructure;

namespace Trinkhalle.Api.BeverageManagement.UseCases;

public record CreateBeveragePurchaseCommand : IRequest<Result<Guid>>
{
    public Guid BeverageId { get; set; }
    public Guid UserId { get; set; }
}

public record BeveragePurchasedEvent
{
    public Guid Id { get; set; }
    public Guid BeverageId { get; set; }
    public string BeverageName { get; set; } = null!;
    public decimal BeveragePrice { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset PurchasedAt { get; set; }
}

public class CreateBeveragePurchase
{
    private const string FunctionName = $"{nameof(CreateBeveragePurchase)}Function";

    private readonly IMediator _mediator;

    public CreateBeveragePurchase(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "BeverageManagement/PurchaseBeverage")]
        HttpRequestData requestData)
    {
        var command = await requestData.ReadFromJsonAsync<CreateBeveragePurchaseCommand>();

        if (command is null) return requestData.CreateResponse(HttpStatusCode.BadRequest);

        var result = await _mediator.Send(command);

        if (result.IsFailed) return await requestData.CreateResponseAsync(HttpStatusCode.BadRequest, result);

        return await requestData.CreateResponseAsync(HttpStatusCode.OK, result);
    }

    public sealed class CreateBeveragePurchaseCommandValidator : AbstractValidator<CreateBeveragePurchaseCommand>
    {
        public CreateBeveragePurchaseCommandValidator()
        {
            RuleFor(x => x.BeverageId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    public class CreateBeveragePurchaseCommandHandler : IRequestHandler<CreateBeveragePurchaseCommand, Result<Guid>>
    {
        private readonly IServicebusEventSender<BeveragePurchasedEvent> _eventSender;
        private readonly TrinkhalleDbContext _dbContext;

        public CreateBeveragePurchaseCommandHandler(TrinkhalleDbContext dbContext,
            IServicebusEventSender<BeveragePurchasedEvent> eventSender)
        {
            _eventSender = eventSender;
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> Handle(CreateBeveragePurchaseCommand request,
            CancellationToken cancellationToken)
        {
            var existing = await _dbContext.Beverages.FindAsync(new object?[] { request.BeverageId },
                cancellationToken: cancellationToken);

            if (existing is null) return Result.Fail("Beverage not found");

            var purchasedEvent = new BeveragePurchasedEvent()
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