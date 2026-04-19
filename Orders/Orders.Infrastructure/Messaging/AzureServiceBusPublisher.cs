using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Orders.Application.Abstractions.Messaging;
using System.Text.Json;

namespace Orders.Infrastructure.Messaging;

public sealed class AzureServiceBusPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;

    public AzureServiceBusPublisher(IOptions<ServiceBusSettings> settings)
    {
        var client = new ServiceBusClient(settings.Value.ConnectionString);
        _sender = client.CreateSender(settings.Value.QueueName);
    }

    public async Task PublishAsync<T>(T message) where T : class
    {
        var body = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            Subject = typeof(T).Name
        };
        await _sender.SendMessageAsync(serviceBusMessage);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
    }
}
