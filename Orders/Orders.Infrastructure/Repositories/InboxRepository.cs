using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions.Repositories;
using Orders.Infrastructure.Messaging;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Repositories;

public class InboxRepository : IInboxRepository
{
    private readonly OrdersDbContext _dbContext;

    public InboxRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
        Console.WriteLine($" InboxRepository DbContext hash: {_dbContext.GetHashCode()}");

    }

    public async Task<bool> ExistsAsync(
        Guid messageId,
        string consumer,
        CancellationToken ct)
    {
        Console.WriteLine(" Checking inbox existence");

        return await _dbContext.InboxMessages
            .AnyAsync(x =>
                x.MessageId == messageId &&
                x.Consumer == consumer,
                ct);
    }

 
    public async Task AddAsync(
    Guid messageId,
    string consumer,
    Guid? correlationId,
    DateTime processedAt,
    CancellationToken ct)
    {
        _dbContext.InboxMessages.Add(new InboxMessage
        {
            MessageId = messageId,
            Consumer = consumer,
            CorrelationId = correlationId,
            ProcessedAt = processedAt
        });

        // SaveChanges is done by OrderRepository.UpdateAsync

        await _dbContext.SaveChangesAsync(ct);
    }

}
