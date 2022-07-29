using System.Net;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Trinkhalle.Api.Shared.Extensions;

namespace Trinkhalle.Api.BeverageManagement.UseCases;

public record LoadBeveragesModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = null!;
    public bool Available { get; init; }
}

public record LoadBeverageQuery : IRequest<Result<IEnumerable<LoadBeveragesModel>>>;

public class LoadBeverages
{
    private const string FunctionName = $"{nameof(LoadBeverages)}Function";

    private readonly IMediator _mediator;

    public LoadBeverages(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(FunctionName)]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "BeverageManagement/LoadBeverages")]
        HttpRequestData requestData)
    {
        var result = await _mediator.Send(new LoadBeverageQuery());

        if (result.IsFailed) return await requestData.CreateResponseAsync(HttpStatusCode.BadRequest, result);

        return await requestData.CreateResponseAsync(HttpStatusCode.OK, result);
    }

    public class LoadBeveragesQueryValidator : AbstractValidator<LoadBeverageQuery>
    {
    }

    public class LoadBeveragesQueryHandler : IRequestHandler<LoadBeverageQuery, Result<IEnumerable<LoadBeveragesModel>>>
    {
        private readonly TrinkhalleDbContext _dbDbContext;

        public LoadBeveragesQueryHandler(TrinkhalleDbContext dbContext)
        {
            _dbDbContext = dbContext;
        }

        public async Task<Result<IEnumerable<LoadBeveragesModel>>> Handle(LoadBeverageQuery request,
            CancellationToken cancellationToken)
        {
            var beverages = await _dbDbContext.Beverages.ToListAsync(cancellationToken: cancellationToken);

            var beveragesResponse = beverages.Select(b => new LoadBeveragesModel()
                { Id = b.Id, Available = b.Available, Name = b.Name, Price = b.Price, ImageUrl = b.ImageUrl });

            return Result.Ok(beveragesResponse);
        }
    }
}