using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;


namespace Trinkhalle.Api.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceBusEventSender<T>(this IServiceCollection services)
    {
        services.AddScoped<IServicebusEventSender<T>>(c =>
        {
            var client = c.GetRequiredService<ServiceBusClient>();
            var sender = client.CreateSender(typeof(T).Name);
            return new ServiceBusEventSender<T>(sender);
        });
        return services;
    }
}