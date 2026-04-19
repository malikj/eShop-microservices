namespace Catalog.Infrastructure.Messaging;

public sealed class ServiceBusSettings
{
    public const string SectionName = "ServiceBus";

    public string ConnectionString { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;  // Queue name (Basic) or Topic name (Standard)
}
