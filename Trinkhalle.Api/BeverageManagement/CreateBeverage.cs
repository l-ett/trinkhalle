using System.Net;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Trinkhalle.Api.Infrastructure;

namespace Trinkhalle.Api.BeverageManagement;

public class CreateBeverage
{
    private const string FunctionName = $"{nameof(CreateBeverage)}Function";

    public record CreateBeverageCommand : IRequest<Guid>
    {
        public string Name { get; init; } = null!;
        public decimal Price { get; init; }
        public string ImageUrl { get; init; } = null!;
        public bool Available { get; init; }
    }

    public record BeverageCreatedEvent
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public decimal Price { get; init; }
        public string ImageUrl { get; init; } = null!;
        public bool Available { get; init; }
    }

    private readonly IMediator _mediator;

    public CreateBeverage(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task<Guid> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "BeverageManagement/CreateBeverage")]
        HttpRequestData requestData)
    {
        var publishBeverageCreatedEventCommand =
            await requestData.ReadFromJsonAsync<CreateBeverageCommand>();

        if (publishBeverageCreatedEventCommand is null) return Guid.Empty;
        var id = await _mediator.Send(publishBeverageCreatedEventCommand);

        return id;
    }

    public class CreateBeverageCommandHandler : IRequestHandler<CreateBeverageCommand, Guid>
    {
        private readonly IServicebusEventSender<BeverageCreatedEvent> _eventSender;

        public CreateBeverageCommandHandler(IServicebusEventSender<BeverageCreatedEvent> eventSender)
        {
            _eventSender = eventSender;
        }

        public async Task<Guid> Handle(CreateBeverageCommand request, CancellationToken cancellationToken)
        {
            var beverageCreatedEvent = new BeverageCreatedEvent()
            {
                Id = Guid.NewGuid(), Available = request.Available, Name = request.Name,
                Price = request.Price, ImageUrl = request.ImageUrl
            };

            await _eventSender.Sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(beverageCreatedEvent)),
                cancellationToken);

            return beverageCreatedEvent.Id;
        }
    }
}