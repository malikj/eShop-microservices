using Orders.Application.Abstractions.Messaging;
using Orders.Application.Abstractions.Correlation;
using Orders.Infrastructure.Persistence;
using System.Diagnostics;
using System.Text.Json;

namespace Orders.Infrastructure.Messaging;

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly OrdersDbContext _dbContext;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    private static readonly ActivitySource ActivitySource = new("OrdersService");

    public MassTransitEventPublisher(
        ICorrelationIdAccessor correlationIdAccessor,
        OrdersDbContext dbContext)
    {
        _correlationIdAccessor = correlationIdAccessor;
        _dbContext = dbContext;
    }

    public async Task PublishAsync<T>(T message) where T : class
    {
        Console.WriteLine("OUTBOX WRITE STARTED");

        Console.WriteLine($"Publish TraceId BEFORE: {Activity.Current?.TraceId}");

        using var activity = ActivitySource.StartActivity("PublishEvent");

        Console.WriteLine($"Publish TraceId AFTER: {Activity.Current?.TraceId}");

      
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(T).AssemblyQualifiedName!,   
            Content = JsonSerializer.Serialize(message)
        };


        _dbContext.OutboxMessages.Add(outboxMessage);
        await _dbContext.SaveChangesAsync();

        Console.WriteLine("OUTBOX WRITE Completed");

    }
}
