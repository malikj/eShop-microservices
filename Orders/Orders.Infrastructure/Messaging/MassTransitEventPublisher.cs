using MassTransit;
using Orders.Application.Abstractions.Correlation;
using Orders.Application.Abstractions.Messaging;
using System.Diagnostics;


namespace Orders.Infrastructure.Messaging;

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    //private static readonly ActivitySource ActivitySource =
    //new ActivitySource("Orders.Messaging");

    private static readonly ActivitySource ActivitySource = new("OrdersService");

    public MassTransitEventPublisher(
        IPublishEndpoint publishEndpoint,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        _publishEndpoint = publishEndpoint;
        _correlationIdAccessor = correlationIdAccessor;
    }

    public Task PublishAsync<T>(T message) where T : class
    {

       // Console.WriteLine($"Publish TraceId: {Activity.Current?.TraceId}");

       // using var activity = ActivitySource.StartActivity(
       //"PublishEvent",
       //ActivityKind.Producer);

       // Console.WriteLine($"Orders TraceId: {Activity.Current?.TraceId}");

        Console.WriteLine($"Publish TraceId BEFORE: {Activity.Current?.TraceId}");

        using var activity = ActivitySource.StartActivity("PublishEvent");

        Console.WriteLine($"Publish TraceId AFTER: {Activity.Current?.TraceId}");


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
