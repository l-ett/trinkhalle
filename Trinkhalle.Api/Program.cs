using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trinkhalle.Api.BeverageManagement;
using Trinkhalle.Api.Infrastructure;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, serviceCollection) =>
    {
        var config = context.Configuration;

        serviceCollection
            .AddMediatR(Assembly.GetExecutingAssembly())
            .AddDbContext<TrinkhalleContext>(
                options => options.UseCosmos(
                    connectionString: config.GetConnectionString("CosmosDb"),
                    databaseName: "Trinkhalle"))
            .AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(config.GetConnectionString("AzureServiceBus"));
            });

        serviceCollection.AddServiceBusEventSender<CreateBeverage.BeverageCreatedEvent>();
    })
    .Build();

host.Run();