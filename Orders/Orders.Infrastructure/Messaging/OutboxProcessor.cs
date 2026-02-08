using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Infrastructure.Persistence;
using System.Text.Json;

namespace Orders.Infrastructure.Messaging;

public class OutboxProcessor : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<OutboxProcessor> _logger;

	public OutboxProcessor(
		IServiceScopeFactory scopeFactory,
		ILogger<OutboxProcessor> logger)
	{
		_scopeFactory = scopeFactory;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Outbox Processor started");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await ProcessOutboxMessages(stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while processing outbox");
			}

			await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
		}
	}

	private async Task ProcessOutboxMessages(CancellationToken cancellationToken)
	{
		using var scope = _scopeFactory.CreateScope();

		var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
		var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

		var messages = await dbContext.OutboxMessages
			.Where(x => x.ProcessedOnUtc == null)
			.OrderBy(x => x.OccurredOnUtc)
			.Take(20)
			.ToListAsync(cancellationToken);

		if (!messages.Any())
			return;

		_logger.LogInformation("Processing {Count} outbox messages", messages.Count);

		foreach (var message in messages)
		{
			try
			{
				// Resolve type
				var type = Type.GetType(message.Type);


				if (type == null)
				{
					_logger.LogError("Unknown message type: {Type}", message.Type);
					message.Error = "Type not found";
					continue;
				}

				// Deserialize
				var @event = JsonSerializer.Deserialize(message.Content, type);

				if (@event == null)
				{
					_logger.LogError("Deserialization failed for {Id}", message.Id);
					message.Error = "Deserialization failed";
					continue;
				}

				// Publish to RabbitMQ
				await publishEndpoint.Publish(@event, cancellationToken);

				// Mark processed
				message.ProcessedOnUtc = DateTime.UtcNow;
				message.Error = null;

				_logger.LogInformation("Outbox message {Id} published", message.Id);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to publish outbox message {Id}", message.Id);
				message.Error = ex.Message;
			}
		}

		await dbContext.SaveChangesAsync(cancellationToken);
	}
}
