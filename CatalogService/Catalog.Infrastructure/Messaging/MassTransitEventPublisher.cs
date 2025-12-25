using Catalog.Application.Abstractions.Messaging;
using MassTransit;

namespace Catalog.Infrastructure.Messaging;

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<T>(T message)
        where T : class
    {
        await _publishEndpoint.Publish(message);
    }
}
