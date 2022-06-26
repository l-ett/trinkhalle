using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Trinkhalle.Api.Domain;
using Trinkhalle.Api.Infrastructure;

namespace Trinkhalle.Api.BeverageManagement;

public record CreateBeverageCommand : IRequest<Guid>
{
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = null!;
    public bool Available { get; init; }
}

public class CreateBeverage
{
    private readonly IMediator _mediator;

    public CreateBeverage(IMediator mediator)
    {
        _mediator = mediator;
    }

    [FunctionName("CreateBeverage")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        CreateBeverageCommand createBeverageCommand)
    {
        var beverageId = await _mediator.Send(createBeverageCommand);

        return new OkObjectResult(beverageId);
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
            var beverage = new Beverage(Guid.NewGuid(), request.Price, request.Name, request.ImageUrl,
                request.Available);

            _dbContext.Beverages.Add(beverage);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return beverage.Id;
        }
    }
}