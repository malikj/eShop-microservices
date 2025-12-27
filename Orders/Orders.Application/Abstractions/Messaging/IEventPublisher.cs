namespace Orders.Application.Abstractions.Messaging;

public interface IEventPublisher
{
	Task PublishAsync<T>(T message) where T : class;
}
