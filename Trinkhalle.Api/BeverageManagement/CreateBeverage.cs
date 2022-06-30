using System.Net;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Trinkhalle.Api.Domain;
using Trinkhalle.Api.Infrastructure;

namespace Trinkhalle.Api.BeverageManagement;

public class CreateBeverage
{
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
    private const string BeverageCreatedEventSubscriber = "BeverageManagement_BeverageCreatedEventConsumer";

    public CreateBeverage(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function("CreateBeverage")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "BeverageManagement/CreateBeverage")]
        HttpRequestData requestData)
    {
        var publishBeverageCreatedEventCommand =
            await requestData.ReadFromJsonAsync<CreateBeverageCommand>();

        if (publishBeverageCreatedEventCommand is null) return requestData.CreateResponse(HttpStatusCode.BadRequest);

        var id = await _mediator.Send(publishBeverageCreatedEventCommand);

        var response = requestData.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(id);

        return response;
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
    
    public record StoreBeverageCommand : IRequest<Guid>
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public decimal Price { get; init; }
        public string ImageUrl { get; init; } = null!;
        public bool Available { get; init; }
    }

    [Function(BeverageCreatedEventSubscriber)]
    public async Task Run(
        [ServiceBusTrigger(topicName: nameof(BeverageCreatedEvent), subscriptionName: BeverageCreatedEventSubscriber,
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

    public class StoreBeverageCommandHandler : IRequestHandler<StoreBeverageCommand, Guid>
    {
        private readonly TrinkhalleContext _dbContext;

        public StoreBeverageCommandHandler(TrinkhalleContext context)
        {
            _dbContext = context;
        }

        public async Task<Guid> Handle(StoreBeverageCommand request, CancellationToken cancellationToken)
        {
            var beverage = new Beverage(request.Id, request.Price, request.Name, request.ImageUrl, request.Available);

            _dbContext.Beverages.Add(beverage);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return beverage.Id;
        }
    }
}