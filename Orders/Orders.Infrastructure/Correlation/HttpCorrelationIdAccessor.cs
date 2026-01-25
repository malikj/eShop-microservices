using Microsoft.AspNetCore.Http;
using Orders.Application.Abstractions.Correlation;

namespace Orders.Infrastructure.Correlation;

public sealed class HttpCorrelationIdAccessor : ICorrelationIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.Items.TryGetValue("CorrelationId", out var value) == true &&
            value is Guid correlationId)
        {
            return correlationId;
        }

        return Guid.Empty;
    }
}
