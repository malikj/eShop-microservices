using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Orders.Infrastructure.Persistence;

using Orders.Application.Abstractions.Messaging;
using Microsoft.Extensions.Hosting;


public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishPendingMessages(stoppingToken);
            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task PublishPendingMessages(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var messages = await db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .Take(20)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            try
            {
                var type = Type.GetType(message.Type)!;
                var obj = JsonSerializer.Deserialize(message.Content, type)!;

                // Use the IEventPublisher abstraction
                await eventPublisher.PublishAsync((object)obj);

                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
