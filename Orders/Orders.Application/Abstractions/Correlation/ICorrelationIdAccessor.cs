namespace Orders.Application.Abstractions.Correlation;

public interface ICorrelationIdAccessor
{
    Guid GetCorrelationId();
}

