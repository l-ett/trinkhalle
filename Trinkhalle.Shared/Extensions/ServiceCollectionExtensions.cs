using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Trinkhalle.Shared.Infrastructure;

namespace Trinkhalle.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServiceBusEventSender<T>(this IServiceCollection services)
    {
        services.AddScoped<IServicebusEventSender<T>>(c =>
        {
            var client = c.GetRequiredService<ServiceBusClient>();
            var sender = client.CreateSender(typeof(T).Name);
            return new ServiceBusEventSender<T>(sender);
        });
    }
}