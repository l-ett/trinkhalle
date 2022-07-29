using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trinkhalle.Api.BeverageManagement;
using Trinkhalle.Api.BeverageManagement.UseCases;
using Trinkhalle.Api.CustomerManagement.UseCases;
using Trinkhalle.Api.Shared;
using Trinkhalle.Api.Shared.Extensions;
using Trinkhalle.Api.Shared.Infrastructure;

namespace Trinkhalle.Api;

public static class Startup
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
    {
        var config = context.Configuration;

        serviceCollection
            .AddMediatR(Assembly.GetExecutingAssembly())
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddDbContext<TrinkhalleDbContext>(
                options => options.UseCosmos(
                    connectionString: config.GetConnectionString("CosmosDb"),
                    databaseName: "Trinkhalle"))
            .AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(config.GetConnectionString("AzureServiceBus"));
            });

        serviceCollection.AddServiceBusEventSender<BeverageCreatedEvent>();
        serviceCollection.AddServiceBusEventSender<BeveragePurchasedEvent>();
        serviceCollection.AddServiceBusEventSender<InvoiceCreatedEvent>();
    }
}