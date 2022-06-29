using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using Trinkhalle.Api.Domain;
using Trinkhalle.Api.Infrastructure;

namespace Trinkhalle.Api.BeverageManagement;

public class CreateBeverage
{
    public record BeverageCreatedEvent
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public decimal Price { get; init; }
        public string ImageUrl { get; init; } = null!;
        public bool Available { get; init; }
    }

    public record CreateBeverageCommand : IRequest<Guid>
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public decimal Price { get; init; }
        public string ImageUrl { get; init; } = null!;
        public bool Available { get; init; }
    }

    private readonly IMediator _mediator;
    private const string BeverageCreatedTopic = "BeverageCreated";
    private const string BeverageCreatedEventSubscriber = "BeverageManagement_BeverageCreatedEventConsumer";

    public CreateBeverage(IMediator mediator)
    {
        _mediator = mediator;
    }

    [FunctionName("BeverageManagement_CreateBeverage")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "BeverageManagement/CreateBeverage")]
        CreateBeverageCommand createBeverageCommand, CancellationToken cancellationToken,
        [ServiceBus(queueOrTopicName: BeverageCreatedTopic, EntityType = ServiceBusEntityType.Topic,
            Connection = "AzureServiceBus")]
        ServiceBusSender sender)
    {
        var beverageCreatedEvent = new BeverageCreatedEvent()
        {
            Id = Guid.NewGuid(), Available = createBeverageCommand.Available, Name = createBeverageCommand.Name,
            Price = createBeverageCommand.Price, ImageUrl = createBeverageCommand.ImageUrl
        };

        await sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(beverageCreatedEvent)), cancellationToken);

        return new OkObjectResult(beverageCreatedEvent.Id);
    }

    [FunctionName(BeverageCreatedEventSubscriber)]
    public async Task Run(
        [ServiceBusTrigger(topicName: BeverageCreatedTopic, subscriptionName: BeverageCreatedEventSubscriber,
            Connection = "AzureServiceBus")]
        BeverageCreatedEvent beverageCreatedEvent, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new CreateBeverageCommand()
            {
                Id = beverageCreatedEvent.Id, Available = beverageCreatedEvent.Available,
                Name = beverageCreatedEvent.Name, ImageUrl = beverageCreatedEvent.ImageUrl,
                Price = beverageCreatedEvent.Price
            }, cancellationToken);
    }

    public class CreateBeverageCommandHandler : IRequestHandler<CreateBeverageCommand, Guid>
    {
        private readonly TrinkhalleContext _dbContext;

        public CreateBeverageCommandHandler(TrinkhalleContext context)
        {
            _dbContext = context;
        }

        public async Task<Guid> Handle(CreateBeverageCommand request, CancellationToken cancellationToken)
        {
            var beverage = new Beverage(request.Id, request.Price, request.Name, request.ImageUrl, request.Available);

            _dbContext.Beverages.Add(beverage);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return beverage.Id;
        }
    }
}