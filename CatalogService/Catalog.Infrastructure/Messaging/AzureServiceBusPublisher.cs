using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Catalog.Application.Abstractions.Messaging;
using Catalog.Application.Checkout.Dtos;
using eShop.Contracts.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Catalog.Infrastructure.Messaging;

public sealed class AzureServiceBusPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<AzureServiceBusPublisher> _logger;

    public AzureServiceBusPublisher(
        IOptions<ServiceBusSettings> settings,
        ILogger<AzureServiceBusPublisher> logger)
    {
        _logger = logger;

        var client = new ServiceBusClient(
            settings.Value.ConnectionString,
            new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    MaxDelay = TimeSpan.FromSeconds(10)
                }
            });

        _sender = client.CreateSender(settings.Value.EntityName);
    }

    public async Task PublishOrderRequestedAsync(
        Guid orderId,
        Guid customerId,
        DateTime createdAt,
        decimal totalAmount,
        IReadOnlyList<OrderItemDto> items)
    {
        var orderRequested = new OrderRequested(
            orderId,
            customerId,
            createdAt,
            totalAmount,
            items);

        var messageBody = JsonSerializer.Serialize(orderRequested);

        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            Subject = nameof(OrderRequested),
            MessageId = orderId.ToString()
        };

        try
        {
            await _sender.SendMessageAsync(message);
            _logger.LogInformation(
                "Published {EventType} for OrderId {OrderId}",
                nameof(OrderRequested), orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish {EventType} for OrderId {OrderId}",
                nameof(OrderRequested), orderId);

            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
    }
}
