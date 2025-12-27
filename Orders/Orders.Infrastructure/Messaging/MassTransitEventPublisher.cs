using MassTransit;
using Orders.Application.Abstractions.Messaging;

namespace Orders.Infrastructure.Messaging;

public class MassTransitEventPublisher : IEventPublisher
{
	private readonly IPublishEndpoint _publishEndpoint;

	public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
	{
		_publishEndpoint = publishEndpoint;
	}

	public Task PublishAsync<T>(T message)
		where T : class
	{
		return _publishEndpoint.Publish(message);
	}
}
