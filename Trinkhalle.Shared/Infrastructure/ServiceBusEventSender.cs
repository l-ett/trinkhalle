using Azure.Messaging.ServiceBus;

namespace Trinkhalle.Shared.Infrastructure;

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