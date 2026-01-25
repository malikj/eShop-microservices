namespace Orders.Application.Abstractions.Repositories;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(
        Guid messageId,
        string consumer,
        CancellationToken ct);

    Task AddAsync(
        Guid messageId,
        string consumer,
        Guid? correlationId,
        DateTime processedAt,
        CancellationToken ct);
}
