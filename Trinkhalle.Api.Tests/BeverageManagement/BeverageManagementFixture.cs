using FakeItEasy;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trinkhalle.Api.BeverageManagement;
using Trinkhalle.Api.Infrastructure;

namespace Trinkhalle.Api.Tests.BeverageManagement;

public class BeverageManagementFixture
{
    private static readonly IServiceProvider _rootContainer;
    private static readonly IServiceScopeFactory _scopeFactory = null!;

    static BeverageManagementFixture()
    {
        var context = new HostBuilderContext(new Dictionary<object, object>());
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {
                    "ConnectionStrings:CosmosDb", ""
                }
            })
            .Build();
        context.Configuration = configuration;

        var services = new ServiceCollection();
        Startup.ConfigureServices(context, services);

        services.AddScoped(sc => A.Fake<IServicebusEventSender<CreateBeverage.BeverageCreatedEvent>>());
        _rootContainer = services.BuildServiceProvider();
        _scopeFactory = _rootContainer.GetService<IServiceScopeFactory>()!;
    }

    public Task SendAsync(IRequest request)
    {
        return ExecuteScopeAsync(sp =>
        {
            var mediator = sp.GetService<IMediator>();

            return mediator.Send(request);
        });
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return ExecuteScopeAsync(sp =>
        {
            var mediator = sp.GetService<IMediator>();

            return mediator.Send(request);
        });
    }

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = _scopeFactory.CreateScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = _scopeFactory.CreateScope();
        var result = await action(scope.ServiceProvider);
        return result;
    }
}