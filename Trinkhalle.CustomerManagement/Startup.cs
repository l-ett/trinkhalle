using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trinkhalle.CustomerManagement.Infrastructure;
using Trinkhalle.Shared.Events;
using Trinkhalle.Shared.Extensions;
using Trinkhalle.Shared.Infrastructure;

namespace Trinkhalle.CustomerManagement;

public static class Startup
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
    {
        var config = context.Configuration;

        serviceCollection
            .AddMediatR(Assembly.GetExecutingAssembly())
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddDbContext<CustomerManagementDbContext>(
                options => options.UseCosmos(
                    connectionString: config.GetConnectionString("CosmosDb"),
                    databaseName: "Trinkhalle"))
            .AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(config.GetConnectionString("AzureServiceBus"));
            });

        serviceCollection.AddServiceBusEventSender<InvoiceCreatedEvent>();
    }
}