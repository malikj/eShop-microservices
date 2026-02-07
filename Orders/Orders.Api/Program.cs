using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions.Repositories;
using Orders.Application.Ordering.Commands.CreateOrder;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Repositories;
using Orders.Api.Consumers;
using Orders.Application.Ordering.Commands.PayOrder;

using Orders.Application.Abstractions.Messaging;
using Orders.Infrastructure.Messaging;
using Serilog;

using Orders.Application.Abstractions.Correlation;
using Orders.Infrastructure.Correlation;
using Orders.Api.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog.Enrichers.Span;



var builder = WebApplication.CreateBuilder(args);

// configure Serilog

builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithSpan()   // IMPORTANT
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] " +
            "TraceId={TraceId} SpanId={SpanId} " +
            "{Message:lj}{NewLine}{Exception}");
});



// --------------------
// Database
// --------------------
builder.Services.AddDbContext<OrdersDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("OrdersDb"));
});


builder.Services.AddHttpContextAccessor();

// --------------------
// Repositories
// --------------------
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderReadRepository, OrderReadRepository>();
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
builder.Services.AddScoped<IInboxRepository, InboxRepository>();


builder.Services.AddScoped<ICorrelationIdAccessor, HttpCorrelationIdAccessor>();



// --------------------
// MediatR (REGISTER ONCE)
// --------------------

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(PayOrderCommand).Assembly);
});




builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderRequestedConsumer>();
    x.AddConsumer<PaymentSucceededConsumer>();
    x.AddConsumer<PaymentFailedConsumer>();


    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        //  GLOBAL retry policy (applies to all consumers)
        cfg.UseMessageRetry(r =>
        {
            //  Do NOT retry domain/business exceptions
            r.Ignore<InvalidOperationException>();
            r.Ignore<ArgumentException>();
            r.Ignore<ArgumentNullException>();

            //  Retry only transient failures
            r.Incremental(
                retryLimit: 5,
                initialInterval: TimeSpan.FromSeconds(1),
                intervalIncrement: TimeSpan.FromSeconds(2));
        });
        cfg.ConfigureEndpoints(context);
    });
});


builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("Orders.Messaging")
            .AddSource("MassTransit")
             .AddSource("MassTransit.Transport")   // ADD THIS
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("OrdersService"))
            .SetSampler(new AlwaysOnSampler())
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddHttpClientInstrumentation()
            .AddMassTransitInstrumentation()
            .AddOtlpExporter(options =>
            {
                //options.Endpoint = new Uri("http://localhost:4318/v1/traces");
                options.Endpoint = new Uri("http://host.docker.internal:4318/v1/traces");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            });
    });


// --------------------
// API
// --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

Console.WriteLine(
    builder.Configuration.GetConnectionString("OrdersDb"));


// --------------------
// Middleware
// --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// 2 Register Middleware
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
