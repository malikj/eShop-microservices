namespace Catalog.Application.Abstractions.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(T message)
        where T : class;
}
