using Azure.Messaging.ServiceBus;

namespace Trinkhalle.Api.Infrastructure;

#pragma warning disable S2326
public class ServiceBusEventSender<T> : IServicebusEventSender<T>
{
    public ServiceBusSender Sender { get; }

    public ServiceBusEventSender(ServiceBusSender serviceBusSender)
    {
        Sender = serviceBusSender;
    }
}

public interface IServicebusEventSender<T>
{
    public ServiceBusSender Sender { get; }
}
#pragma warning restore S2326