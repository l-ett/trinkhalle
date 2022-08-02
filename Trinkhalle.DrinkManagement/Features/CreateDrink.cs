using System.Net;
using Azure.Messaging.ServiceBus;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Trinkhalle.Shared.Events;
using Trinkhalle.Shared.Extensions;
using Trinkhalle.Shared.Infrastructure;

namespace Trinkhalle.DrinkManagement.Features;

public record CreateDrinkCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = null!;
    public bool Available { get; init; }
}

public class CreateDrink
{
    private const string FunctionName = $"{nameof(CreateDrink)}Function";

    private readonly IMediator _mediator;

    public CreateDrink(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DrinkManagement/CreateDrink")]
        HttpRequestData requestData)
    {
        var command = await requestData.ReadFromJsonAsync<CreateDrinkCommand>();

        if (command is null) return requestData.CreateResponse(HttpStatusCode.BadRequest);

        var result = await _mediator.Send(command);

        if (result.IsFailed) return await requestData.CreateResponseAsync(HttpStatusCode.BadRequest, result);

        return await requestData.CreateResponseAsync(HttpStatusCode.Created, result);
    }

    public sealed class CreateDrinkCommandValidator : AbstractValidator<CreateDrinkCommand>
    {
        public CreateDrinkCommandValidator()
        {
            RuleFor(x => x.Available).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ImageUrl).NotNull();
        }
    }

    public class CreateDrinkCommandHandler : IRequestHandler<CreateDrinkCommand, Result<Guid>>
    {
        private readonly IServicebusEventSender<DrinkCreatedEvent> _eventSender;

        public CreateDrinkCommandHandler(IServicebusEventSender<DrinkCreatedEvent> eventSender)
        {
            _eventSender = eventSender;
        }

        public async Task<Result<Guid>> Handle(CreateDrinkCommand request, CancellationToken cancellationToken)
        {
            var drinkCreatedEvent = new DrinkCreatedEvent()
            {
                Id = Guid.NewGuid(), Available = request.Available, Name = request.Name,
                Price = request.Price, ImageUrl = request.ImageUrl
            };

            await _eventSender.Sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(drinkCreatedEvent)),
                cancellationToken);

            return Result.Ok(drinkCreatedEvent.Id);
        }
    }
}