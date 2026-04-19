using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.Messaging.Consumers;
using System.Text.Json;
using eShop.Contracts.Events;

namespace Orders.Infrastructure.Messaging;

public class AzureServiceBusConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<AzureServiceBusConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AzureServiceBusConsumer(
        IOptions<ServiceBusSettings> settings,
        ILogger<AzureServiceBusConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var connectionString = settings.Value.ConnectionString;
        var queueName = settings.Value.QueueName;

        _logger.LogInformation("[ServiceBus] Initializing AzureServiceBusConsumer. Queue: {QueueName}, ConnectionString is empty: {IsEmpty}",
            queueName, string.IsNullOrEmpty(connectionString));

        var client = new ServiceBusClient(connectionString);
        _processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

        _logger.LogInformation("[ServiceBus] ServiceBusClient and Processor created for queue: {QueueName}", queueName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ServiceBus] AzureServiceBusConsumer starting. Registering message handlers...");

        _processor.ProcessMessageAsync += ProcessMessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        _logger.LogInformation("[ServiceBus] Starting processor. Listening for messages...");
        await _processor.StartProcessingAsync(stoppingToken);

        _logger.LogInformation("[ServiceBus] Processor started successfully. Waiting for messages...");
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogInformation("[ServiceBus] AzureServiceBusConsumer stopping.");
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        var subject = args.Message.Subject;
        var messageId = args.Message.MessageId;

        _logger.LogInformation("[ServiceBus] Message received. MessageId: {MessageId}, Subject: {Subject}", messageId, subject);
        _logger.LogDebug("[ServiceBus] Message body: {Body}", body);

        using var scope = _serviceProvider.CreateScope();
        try
        {
            switch (subject)
            {
                case nameof(OrderRequested):
                    _logger.LogInformation("[ServiceBus] Routing to OrderRequestedConsumer. MessageId: {MessageId}", messageId);
                    var orderRequested = JsonSerializer.Deserialize<OrderRequested>(body);
                    if (orderRequested == null)
                    {
                        _logger.LogError("[ServiceBus] Failed to deserialize OrderRequested message. MessageId: {MessageId}", messageId);
                        await args.AbandonMessageAsync(args.Message);
                        return;
                    }
                    var orderRequestedConsumer = scope.ServiceProvider.GetRequiredService<OrderRequestedConsumer>();
                    await orderRequestedConsumer.Handle(orderRequested);
                    _logger.LogInformation("[ServiceBus] OrderRequestedConsumer handled successfully. OrderId: {OrderId}, MessageId: {MessageId}", orderRequested.OrderId, messageId);
                    break;

                case nameof(PaymentSucceeded):
                    _logger.LogInformation("[ServiceBus] Routing to PaymentSucceededConsumer. MessageId: {MessageId}", messageId);
                    var paymentSucceeded = JsonSerializer.Deserialize<PaymentSucceeded>(body);
                    if (paymentSucceeded == null)
                    {
                        _logger.LogError("[ServiceBus] Failed to deserialize PaymentSucceeded message. MessageId: {MessageId}", messageId);
                        await args.AbandonMessageAsync(args.Message);
                        return;
                    }
                    var paymentSucceededConsumer = scope.ServiceProvider.GetRequiredService<PaymentSucceededConsumer>();
                    Guid? correlationId = null;
                    if (!string.IsNullOrEmpty(args.Message.CorrelationId) && Guid.TryParse(args.Message.CorrelationId, out var parsedCorrelationId))
                        correlationId = parsedCorrelationId;
                    Guid parsedMessageId;
                    if (!Guid.TryParse(args.Message.MessageId, out parsedMessageId))
                        parsedMessageId = Guid.NewGuid();
                    await paymentSucceededConsumer.Handle(paymentSucceeded, parsedMessageId, correlationId, args.CancellationToken);
                    _logger.LogInformation("[ServiceBus] PaymentSucceededConsumer handled successfully. MessageId: {MessageId}", messageId);
                    break;

                case nameof(PaymentFailed):
                    _logger.LogInformation("[ServiceBus] Routing to PaymentFailedConsumer. MessageId: {MessageId}", messageId);
                    var paymentFailed = JsonSerializer.Deserialize<PaymentFailed>(body);
                    if (paymentFailed == null)
                    {
                        _logger.LogError("[ServiceBus] Failed to deserialize PaymentFailed message. MessageId: {MessageId}", messageId);
                        await args.AbandonMessageAsync(args.Message);
                        return;
                    }
                    var paymentFailedConsumer = scope.ServiceProvider.GetRequiredService<PaymentFailedConsumer>();
                    await paymentFailedConsumer.Handle(paymentFailed, args.CancellationToken);
                    _logger.LogInformation("[ServiceBus] PaymentFailedConsumer handled successfully. MessageId: {MessageId}", messageId);
                    break;

                default:
                    _logger.LogWarning("[ServiceBus] Unknown message subject: {Subject}. MessageId: {MessageId}. Message will be abandoned.", subject, messageId);
                    await args.AbandonMessageAsync(args.Message);
                    return;
            }

            await args.CompleteMessageAsync(args.Message);
            _logger.LogInformation("[ServiceBus] Message completed successfully. MessageId: {MessageId}, Subject: {Subject}", messageId, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ServiceBus] Error processing message. MessageId: {MessageId}, Subject: {Subject}", messageId, subject);
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "[ServiceBus] Error occurred. Source: {ErrorSource}, Namespace: {Namespace}, EntityPath: {EntityPath}",
            args.ErrorSource, args.FullyQualifiedNamespace, args.EntityPath);
        return Task.CompletedTask;
    }
}
