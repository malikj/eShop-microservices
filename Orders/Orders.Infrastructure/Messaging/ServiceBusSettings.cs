
namespace Orders.Infrastructure.Messaging;

public sealed class ServiceBusSettings
{
    public const string SectionName = "ServiceBus";
    public string ConnectionString { get; init; } = string.Empty;
    public string QueueName { get; init; } = string.Empty;
}