using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Orders.Infrastructure.Persistence;


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
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

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

                await publishEndpoint.Publish(obj, ct);

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
