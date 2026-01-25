using MassTransit;
using Orders.Application.Abstractions.Correlation;
using Orders.Application.Abstractions.Messaging;

namespace Orders.Infrastructure.Messaging;

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public MassTransitEventPublisher(
        IPublishEndpoint publishEndpoint,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        _publishEndpoint = publishEndpoint;
        _correlationIdAccessor = correlationIdAccessor;
    }

    public Task PublishAsync<T>(T message) where T : class
    {
        return _publishEndpoint.Publish(message, context =>
        {
            var correlationId = _correlationIdAccessor.GetCorrelationId();
            if (correlationId != Guid.Empty)
            {
                context.CorrelationId = correlationId;
            }
        });
    }
}
